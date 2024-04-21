#include "pch.h"
#include "mkl.h"
#include "SplineOptimization.h"

#include <iostream>

enum class ErrorEnum { NO, INIT, CHECK, SOLVE, JACOBI, GET, DEL, RCI };


struct SplineData {

	int nX;
	const double* boundaries;
	int nY;
	int nS;
	const double* X;
	const double* Y;
	int ErrCode;
	double* splineValues;
	double* scoeff = nullptr;

	SplineData(
		int nX,
		const double* boundaries,
		const int nS,
		const double* X,
		const double* Y,
		double* splineValues
	)
	{
		this->nX = nX;
		this->boundaries = boundaries;
		this->nS = nS;
		this->X = X;
		this->Y = Y;
		this->splineValues = splineValues;
		nY = 1;
		ErrCode = 0;
	}
};

void MakeSpline(MKL_INT* m, MKL_INT* n, double* x, double* f, void* sd)
{
	SplineData* data = (SplineData*)sd;
	int nX = data->nX;
	const double* boundaries = data->boundaries;
	int nY = data->nY;
	int nS = data->nS;
	const double* X = data->X;
	const double* Y = data->Y;
	int& ErrCode = data->ErrCode;
	double* splineValues = data->splineValues;

	MKL_INT s_order = DF_PP_CUBIC;
	MKL_INT s_type = DF_PP_NATURAL;
	MKL_INT bc_type = DF_BC_FREE_END;

	double* scoeff = new double[nY * (nX - 1) * s_order];
	try
	{
		DFTaskPtr task;
		int status = -1;
		status = dfdNewTask1D(&task,
			nX, boundaries,
			DF_UNIFORM_PARTITION,
			nY, x,
			DF_NO_HINT);
		if (status != DF_STATUS_OK) throw 1;

		status = dfdEditPPSpline1D(task,
			s_order, s_type, bc_type, NULL,
			DF_NO_IC,
			NULL,
			scoeff,
			DF_NO_HINT);
		if (status != DF_STATUS_OK) throw 2;

		status = dfdConstruct1D(task,
			DF_PP_SPLINE,
			DF_METHOD_STD);
		if (status != DF_STATUS_OK) throw 3;

		int nDorder = 1;
		MKL_INT dorder[] = { 1 };
		status = dfdInterpolate1D(task,
			DF_INTERP,
			DF_METHOD_PP,
			nS, X,
			DF_SORTED_DATA,
			nDorder, dorder,
			NULL,
			f,
			DF_NO_HINT,
			NULL);
		if (status != DF_STATUS_OK) throw 4;


		for (int i = 0; i < nS; ++i) {
			splineValues[i] = f[i];
			f[i] = Y[i] - f[i];
		}

		status = dfDeleteTask(&task);
		if (status != DF_STATUS_OK) throw 5;
	}
	catch (int ret)
	{
		if (data->scoeff == nullptr)
			data->scoeff = scoeff;
		else {
			delete[]data->scoeff;
			data->scoeff = scoeff;
		}
		ErrCode = ret;
	}

	if (data->scoeff == nullptr)
		data->scoeff = scoeff;
	else {
		delete[]data->scoeff;
		data->scoeff = scoeff;
	}
	ErrCode = 0;
}

void UniformGridValues(SplineData* data, double* x, double* values, int uniform_num_nodes) {
	int nX = data->nX;
	const double* boundaries = data->boundaries;
	int nY = data->nY;
	const double* Y = data->Y;
	int& ErrCode = data->ErrCode;
	double* splineValues = data->splineValues;

	MKL_INT s_order = DF_PP_CUBIC;
	MKL_INT s_type = DF_PP_NATURAL;
	MKL_INT bc_type = DF_BC_FREE_END;

	double* scoeff = new double[nY * (nX - 1) * s_order];
	try
	{
		DFTaskPtr task;
		int status = -1;
		status = dfdNewTask1D(&task,
			nX, boundaries,
			DF_UNIFORM_PARTITION,
			nY, x,
			DF_NO_HINT);
		if (status != DF_STATUS_OK) throw 1;

		status = dfdEditPPSpline1D(task,
			s_order, s_type, bc_type, NULL,
			DF_NO_IC,
			NULL,
			scoeff,
			DF_NO_HINT);
		if (status != DF_STATUS_OK) throw 2;

		status = dfdConstruct1D(task,
			DF_PP_SPLINE,
			DF_METHOD_STD);
		if (status != DF_STATUS_OK) throw 3;

		int nDorder = 1;
		MKL_INT dorder[] = { 1 };
		status = dfdInterpolate1D(task,
			DF_INTERP,
			DF_METHOD_PP,
			uniform_num_nodes, boundaries,
			DF_UNIFORM_PARTITION,
			nDorder, dorder,
			NULL,
			values,
			DF_NO_HINT,
			NULL);
		if (status != DF_STATUS_OK) throw 4;

		status = dfDeleteTask(&task);
		if (status != DF_STATUS_OK) throw 5;
	}
	catch (int ret)
	{
		ErrCode = ret;
	}
}

void SplineOptimization(
	const double* X,
	const double* Y,
	int M,
	int N,
	double* spline,
	int maxiter,
	double stopdis,
	int uniform_num_nodes,
	double* values,
	MKL_INT& numiter,
	int& stop,
	double& resFinal,
	double* scoeff
) {
	double* x = new double[M];

	for (int i = 0; i < M; ++i) x[i] = 0;

	double rs = 10;

	const double eps[6] =
	{ 1e-12 ,
	stopdis ,
	1e-12 ,
	1e-12 ,
	1e-12 ,
	1e-12 };
	double jac_eps = 1.0E-8;

	double res_initial = 0;
	MKL_INT stop_criteria;
	MKL_INT check_data_info[4];
	ErrorEnum error = ErrorEnum(ErrorEnum::NO);

	double* boundaries = new double[2] {X[0], X[N - 1]};

	SplineData sd = SplineData(M, boundaries, N, X, Y, spline);

	_TRNSP_HANDLE_t handle = NULL;
	double* fvec = NULL;
	double* fjac = NULL;
	error = ErrorEnum(ErrorEnum::NO);
	try
	{
		fvec = new double[N];
		fjac = new double[M * N];

		MKL_INT ret = dtrnlsp_init(&handle, &M, &N, x, eps, &maxiter, &maxiter, &rs);
		if (ret != TR_SUCCESS) throw (ErrorEnum(ErrorEnum::INIT));

		ret = dtrnlsp_check(&handle, &M, &N, fjac, fvec, eps, check_data_info);
		if (ret != TR_SUCCESS) throw (ErrorEnum(ErrorEnum::CHECK));

		MKL_INT RCI_Request = 0;

		while (true)
		{
			ret = dtrnlsp_solve(&handle, fvec, fjac, &RCI_Request);
			if (ret != TR_SUCCESS) throw (ErrorEnum(ErrorEnum::SOLVE));
			if (RCI_Request == 0) continue;
			else if (RCI_Request == 1) MakeSpline(&N, &M, x, fvec, &sd);
			else if (RCI_Request == 2)
			{
				ret = djacobix(MakeSpline, &M, &N, fjac, x, &jac_eps, &sd);
				if (ret != TR_SUCCESS) throw (ErrorEnum(ErrorEnum::JACOBI));
			}
			else if (RCI_Request >= -6 && RCI_Request <= -1) break;
			else throw (ErrorEnum(ErrorEnum::RCI));
		}

		ret = dtrnlsp_get(&handle, &numiter, &stop_criteria,
			&res_initial, &resFinal);
		if (ret != TR_SUCCESS) throw (ErrorEnum(ErrorEnum::GET));

		ret = dtrnlsp_delete(&handle);
		if (ret != TR_SUCCESS) throw (ErrorEnum(ErrorEnum::DEL));
	}
	catch (ErrorEnum _error) { error = _error; }

	for (int i = 0; i < (M - 1) * DF_PP_CUBIC; ++i) {
		scoeff[i] = sd.scoeff[i];
	}

	if (fvec != NULL) delete[] fvec;
	if (fjac != NULL) delete[] fjac;
	stop = stop_criteria;
	UniformGridValues(&sd, x, values, uniform_num_nodes);
}
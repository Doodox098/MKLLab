#include "pch.h"
#include "mkl.h"
#include "MYDLL.h"

int CubicSpline(
	int nX, // ����� ����� �������
	const double* X, // ������ ����� �������
	int nY, // ����������� ��������� �������
	const double* Y, // ������ �������� �������� ��������� �������
	const int nS, // ����� ����� ����������� �����, �� �������
	// ����������� �������� ������� � ��� �����������
	double sL, // ����� ����� ����������� �����
	double sR, // ������ ����� ����������� �����
	double* splineValues // ������ ����������� �������� �������
	)
{
	MKL_INT s_order = DF_PP_CUBIC; // ������� ����������� �������
	MKL_INT s_type = DF_PP_NATURAL; // ��� �������
	// ��� ��������� �������
	MKL_INT bc_type = DF_BC_FREE_END;
	// ������ ��� ������������� �������
	double* scoeff = new double[nY * (nX - 1) * s_order];
	try
	{
		DFTaskPtr task;
		int status = -1;
		// C������� ������ (task)
		status = dfdNewTask1D(&task,
			nX, X,
			DF_NON_UNIFORM_PARTITION, // ������������� ����� �����
			nY, Y,
			DF_NO_HINT); // ������ �������� �������� ���������
		// ������� �� ��������� (���������)
		if (status != DF_STATUS_OK) throw 1;
		// ��������� ���������� ������
		status = dfdEditPPSpline1D(task,
			s_order, s_type, bc_type, NULL,
			DF_NO_IC, // ��� ������� �� ���������� ������
			NULL, // ������ �������� ��� ������� �� ���������� ������
			scoeff,
			DF_NO_HINT); // ������ �������� ������������� �������
		// � ���������� ������ (Row-major - ���������)
		if (status != DF_STATUS_OK) throw 2;
		// �������� �������
		status = dfdConstruct1D(task,
			DF_PP_SPLINE, // �������������� ������ ���� ��������
			DF_METHOD_STD); // �������������� ������ ���� ��������
		if (status != DF_STATUS_OK) throw 3;
		// ���������� �������� ������� � ��� �����������
		double grid[2]{ sL, sR };// ������ ������ ����������� �����, �� �������
		// ����������� �������� ������� � �����������
		int nDorder = 1; // ����� �����������, ������� �����������, ���� 1
		MKL_INT dorder[] = { 1}; // ����������� �������� �������
		status = dfdInterpolate1D(task,
			DF_INTERP, // ����������� �������� ������� � ��� �����������
			DF_METHOD_PP, // �������������� ������ ���� ��������
			nS, grid,
			DF_UNIFORM_PARTITION, // �������� ������� � ��� �����������
			// ����������� �� ����������� �����
			nDorder, dorder,
			NULL, // ��� �������������� ���������� �� ����� ������������
			splineValues,
			DF_NO_HINT, // ������ �������� ����������� � ���������� ������
			NULL); // ������������ ��� ��������� ���������� ��
		// ������������� �����; ����� ��������� �������� NULL	
		if (status != DF_STATUS_OK) throw 4;
		// ������������ ��������
		status = dfDeleteTask(&task);
		if (status != DF_STATUS_OK) throw 5;
	}
	catch (int ret)
	{
		delete[] scoeff;
		return ret;
	}
	delete[] scoeff;
	return 0;
}
#pragma once

#ifdef MATHLIBRARY_EXPORTS
#define MYDLL_API __declspec(dllexport)
#else
#define MYDLL_API __declspec(dllimport)
#endif

/*extern "C" MYDLL_API int CubicSpline(
	int nX, // ����� ����� �������
	const double* grid, // ������� ����������� �����
	int nY, // ����������� ��������� �������
	const double* Y, // ������ �������� �������� ��������� �������
	const int nS, // ����� ����� ������������� �����, �� �������
	// ����������� �������� ������� � ��� �����������
	const double* X, // ������������� �����
	double* splineValues // ������ ����������� �������� �������
)*/

extern "C" __declspec(dllexport) void SplineOptimization(
	const double* X,
	const double* Y,
	int M,
	int N,
	double* spline,
	int maxiter,
	double stopdis,
	int uniform_num_nodes,
	double* values,
	MKL_INT & numiter,
	int& errorcode,
	double& resFinal,
	double* scoeff
);

// mkl_intel_c_dll.lib mkl_intel_thread_dll.lib mkl_core_dll.lib libiomp5md.lib
// -I"%MKLROOT%\include"
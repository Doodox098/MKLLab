#pragma once

#ifdef MATHLIBRARY_EXPORTS
#define MYDLL_API __declspec(dllexport)
#else
#define MYDLL_API __declspec(dllimport)
#endif

extern "C" MYDLL_API int CubicSpline(
	int nX, // ����� ����� �������
	const double* X, // ������ ����� �������
	int nY, // ����������� ��������� �������
	const double* Y, // ������ �������� �������� ��������� �������
	const int nS, // ����� ����� ����������� �����, �� �������
	// ����������� �������� ������� � ��� �����������
	double sL, // ����� ����� ����������� �����
	double sR, // ������ ����� ����������� �����
	double* splineValues // ������ ����������� �������� �������
);

// mkl_intel_c_dll.lib mkl_intel_thread_dll.lib mkl_core_dll.lib libiomp5md.lib
// -I"%MKLROOT%\include"
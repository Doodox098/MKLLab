#pragma once

#ifdef MATHLIBRARY_EXPORTS
#define MYDLL_API __declspec(dllexport)
#else
#define MYDLL_API __declspec(dllimport)
#endif

extern "C" MYDLL_API int CubicSpline(
	int nX, // число узлов сплайна
	const double* X, // массив узлов сплайна
	int nY, // размерность векторной функции
	const double* Y, // массив заданных значений векторной функции
	const int nS, // число узлов равномерной сетки, на которой
	// вычисляются значения сплайна и его производных
	double sL, // левый конец равномерной сетки
	double sR, // правый конец равномерной сетки
	double* splineValues // массив вычисленных значений сплайна
);

// mkl_intel_c_dll.lib mkl_intel_thread_dll.lib mkl_core_dll.lib libiomp5md.lib
// -I"%MKLROOT%\include"
// Kernel code
// Matrices are stored in row-major order:
// M(row, col) = *(M.elements + row * M.width + col)
TYPEdef struct {
 int width;
 int height;
 __global float* elements;
} Matrix;
// Thread block size
#define BLOCK_SIZE 32
// Matrix multiplication function called by MatMulKernel()
void MatMul(Matrix A, Matrix B, Matrix C)
{
 float Cvalue = 0;
 int row = get_global_id(1);
 int col = get_global_id(0);
 for (int e = 0; e < A.width; ++e)
 Cvalue += A.elements[row * A.width + e] * B.elements[e * B.width + col];
 C.elements[row * C.width + col] = Cvalue;
}
// Matrix multiplication kernel called by MatMulHost()
__kernel void MatMulKernel(
 int Awidth, int Aheight, __global float* Aelements,
 int Bwidth, int Bheight, __global float* Belements,
 int Cwidth, int Cheight, __global float* Celements)
{
 Matrix A = { Awidth, Aheight, Aelements };
 Matrix B = { Bwidth, Bheight, Belements };
 Matrix C = { Cwidth, Cheight, Celements };
 MatMul(A, B, C);
}
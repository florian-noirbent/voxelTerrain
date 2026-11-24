using UnityEngine;

static class SimplexNoise
{
	static public int simplexSeed;
	
	static int FASTFLOOR(float x) 
	{ 
		return (x > 0) ? ((int)x) : (((int)x)-1); 
	}

	static int hash(int a)
	{
	    a = (a ^ 61) ^ (a >> 16);
	    a = a + (a << 3);
	    a = a ^ (a >> 4);
	    a = a * simplexSeed;
	    a = a ^ (a >> 15);
	    return a;
	}

	static float[,] grad2lut = new float[8,2]
	{
	  { -1.0f, -1.0f }, { 1.0f, 0.0f } , { -1.0f, 0.0f } , { 1.0f, 1.0f } ,
	  { -1.0f, 1.0f } , { 0.0f, -1.0f } , { 0.0f, 1.0f } , { 1.0f, -1.0f }
	};
	
	static float[,] grad3lut = new float[16,3]
	{
	  { 1.0f, 0.0f, 1.0f }, { 0.0f, 1.0f, 1.0f },
	  { -1.0f, 0.0f, 1.0f }, { 0.0f, -1.0f, 1.0f },
	  { 1.0f, 0.0f, -1.0f }, { 0.0f, 1.0f, -1.0f },
	  { -1.0f, 0.0f, -1.0f }, { 0.0f, -1.0f, -1.0f },
	  { 1.0f, -1.0f, 0.0f }, { 1.0f, 1.0f, 0.0f },
	  { -1.0f, 1.0f, 0.0f }, { -1.0f, -1.0f, 0.0f },
	  { 1.0f, 0.0f, 1.0f }, { -1.0f, 0.0f, 1.0f },
	  { 0.0f, 1.0f, -1.0f }, { 0.0f, -1.0f, -1.0f }
	};
	
	static float[,] grad4lut = new float[32,4]
	{
	  { 0.0f, 1.0f, 1.0f, 1.0f }, { 0.0f, 1.0f, 1.0f, -1.0f }, { 0.0f, 1.0f, -1.0f, 1.0f }, { 0.0f, 1.0f, -1.0f, -1.0f },
	  { 0.0f, -1.0f, 1.0f, 1.0f }, { 0.0f, -1.0f, 1.0f, -1.0f }, { 0.0f, -1.0f, -1.0f, 1.0f }, { 0.0f, -1.0f, -1.0f, -1.0f },
	  { 1.0f, 0.0f, 1.0f, 1.0f }, { 1.0f, 0.0f, 1.0f, -1.0f }, { 1.0f, 0.0f, -1.0f, 1.0f }, { 1.0f, 0.0f, -1.0f, -1.0f },
	  { -1.0f, 0.0f, 1.0f, 1.0f }, { -1.0f, 0.0f, 1.0f, -1.0f }, { -1.0f, 0.0f, -1.0f, 1.0f }, { -1.0f, 0.0f, -1.0f, -1.0f },
	  { 1.0f, 1.0f, 0.0f, 1.0f }, { 1.0f, 1.0f, 0.0f, -1.0f }, { 1.0f, -1.0f, 0.0f, 1.0f }, { 1.0f, -1.0f, 0.0f, -1.0f },
	  { -1.0f, 1.0f, 0.0f, 1.0f }, { -1.0f, 1.0f, 0.0f, -1.0f }, { -1.0f, -1.0f, 0.0f, 1.0f }, { -1.0f, -1.0f, 0.0f, -1.0f },
	  { 1.0f, 1.0f, 1.0f, 0.0f }, { 1.0f, 1.0f, -1.0f, 0.0f }, { 1.0f, -1.0f, 1.0f, 0.0f }, { 1.0f, -1.0f, -1.0f, 0.0f },
	  { -1.0f, 1.0f, 1.0f, 0.0f }, { -1.0f, 1.0f, -1.0f, 0.0f }, { -1.0f, -1.0f, 1.0f, 0.0f }, { -1.0f, -1.0f, -1.0f, 0.0f }
	};
	
	static byte[,] simplex = new byte[64,4]
	{
	  {0,1,2,3},{0,1,3,2},{0,0,0,0},{0,2,3,1},{0,0,0,0},{0,0,0,0},{0,0,0,0},{1,2,3,0},
	  {0,2,1,3},{0,0,0,0},{0,3,1,2},{0,3,2,1},{0,0,0,0},{0,0,0,0},{0,0,0,0},{1,3,2,0},
	  {0,0,0,0},{0,0,0,0},{0,0,0,0},{0,0,0,0},{0,0,0,0},{0,0,0,0},{0,0,0,0},{0,0,0,0},
	  {1,2,0,3},{0,0,0,0},{1,3,0,2},{0,0,0,0},{0,0,0,0},{0,0,0,0},{2,3,0,1},{2,3,1,0},
	  {1,0,2,3},{1,0,3,2},{0,0,0,0},{0,0,0,0},{0,0,0,0},{2,0,3,1},{0,0,0,0},{2,1,3,0},
	  {0,0,0,0},{0,0,0,0},{0,0,0,0},{0,0,0,0},{0,0,0,0},{0,0,0,0},{0,0,0,0},{0,0,0,0},
	  {2,0,1,3},{0,0,0,0},{0,0,0,0},{0,0,0,0},{3,0,1,2},{3,0,2,1},{0,0,0,0},{3,1,2,0},
	  {2,1,0,3},{0,0,0,0},{0,0,0,0},{0,0,0,0},{3,1,0,2},{0,0,0,0},{3,2,0,1},{3,2,1,0}
	};
	
	
	static void grad1( int hash, out float gx )
	{
	    int h = hash & 15;
	    gx = 1.0f + (h & 7);
	    if ((h&8) != 0) gx = - gx;
	}
	
	static void grad2( int hash, out float gx, out float gy ) 
	{
	    int h = hash & 7;
	    gx = grad2lut[h, 0];
	    gy = grad2lut[h, 1];
	    return;
	}
	
	static void grad3( int hash, out float gx, out float gy, out float gz )
	{
	    int h = hash & 15;
	    gx = grad3lut[h, 0];
	    gy = grad3lut[h, 1];
	    gz = grad3lut[h, 2];
	    return;
	}
	
	static void grad4( int hash, out float gx, out float gy, out float gz, out float gw)
	{
	    int h = hash & 31;
	    gx = grad4lut[h, 0];
	    gy = grad4lut[h, 1];
	    gz = grad4lut[h, 2];
	    gw = grad4lut[h, 3];
	    return;
	}
	
	public static float sdnoise1( float x, out float dnoise_dx)
	{
	  int i0 = FASTFLOOR(x);
	  int i1 = i0 + 1;
	  float x0 = x - i0;
	  float x1 = x0 - 1.0f;
	
	  float gx0, gx1;
	  float n0, n1;
	  float t20, t40, t21, t41;
	  
	  float x20 = x0*x0;
	  float t0 = 1.0f - x20;
	  t20 = t0 * t0;
	  t40 = t20 * t20;
	  grad1( hash(i0 & 0xff), out gx0);
	  n0 = t40 * gx0 * x0;
	
	  float x21 = x1*x1;
	  float t1 = 1.0f - x21;
	  t21 = t1 * t1;
	  t41 = t21 * t21;
	  grad1( hash(i1 & 0xff), out gx1);
	  n1 = t41 * gx1 * x1;
	
	  dnoise_dx = t20 * t0 * gx0 * x20;
	  dnoise_dx += t21 * t1 * gx1 * x21;
	  dnoise_dx *= -8.0f;
	  dnoise_dx += t40 * gx0 + t41 * gx1;
	  dnoise_dx *= 0.25f;
	
	  return 0.25f * (n0 + n1);
	}
	
	
	const float F2 = 0.366025403f;
	const float G2 =  0.211324865f;
	
	
	public static float sdnoise2( float x, float y, out float dnoise_dx, out float dnoise_dy )
	{
	    float n0, n1, n2;
	    float gx0, gy0, gx1, gy1, gx2, gy2;
	
	    float s = ( x + y ) * F2;
	    float xs = x + s;
	    float ys = y + s;
	    int i = FASTFLOOR( xs );
	    int j = FASTFLOOR( ys );
	
	    float t = ( float ) ( i + j ) * G2;
	    float X0 = i - t;
	    float Y0 = j - t;
	    float x0 = x - X0;
	    float y0 = y - Y0;
	
	    int i1, j1;
	    if( x0 > y0 ) { i1 = 1; j1 = 0; } 
	    else { i1 = 0; j1 = 1; }
	
	    float x1 = x0 - i1 + G2;
	    float y1 = y0 - j1 + G2;
	    float x2 = x0 - 1.0f + 2.0f * G2;
	    float y2 = y0 - 1.0f + 2.0f * G2;
	
	    int ii = i % 256;
	    int jj = j % 256;
	
	    float t0 = 0.5f - x0 * x0 - y0 * y0;
	    float t20, t40;
	    if( t0 < 0.0f ) t40 = t20 = t0 = n0 = gx0 = gy0 = 0.0f;
	    else {
	      grad2(  hash(ii +  hash(jj)), out gx0, out gy0 );
	      t20 = t0 * t0;
	      t40 = t20 * t20;
	      n0 = t40 * ( gx0 * x0 + gy0 * y0 );
	    }
	
	    float t1 = 0.5f - x1 * x1 - y1 * y1;
	    float t21, t41;
	    if( t1 < 0.0f ) t21 = t41 = t1 = n1 = gx1 = gy1 = 0.0f;
	    else {
	      grad2(  hash(ii + i1 +  hash(jj + j1)), out gx1, out gy1 );
	      t21 = t1 * t1;
	      t41 = t21 * t21;
	      n1 = t41 * ( gx1 * x1 + gy1 * y1 );
	    }
	
	    float t2 = 0.5f - x2 * x2 - y2 * y2;
	    float t22, t42;
	    if( t2 < 0.0f ) t42 = t22 = t2 = n2 = gx2 = gy2 = 0.0f;
	    else {
	      grad2(  hash(ii + 1 +  hash(jj + 1)), out gx2, out gy2 );
	      t22 = t2 * t2;
	      t42 = t22 * t22;
	      n2 = t42 * ( gx2 * x2 + gy2 * y2 );
	    }
	
	    float noise = 40.0f * ( n0 + n1 + n2 );
	
        float temp0 = t20 * t0 * ( gx0* x0 + gy0 * y0 );
        dnoise_dx = temp0 * x0;
        dnoise_dy = temp0 * y0;
        float temp1 = t21 * t1 * ( gx1 * x1 + gy1 * y1 );
        dnoise_dx += temp1 * x1;
        dnoise_dy += temp1 * y1;
        float temp2 = t22 * t2 * ( gx2* x2 + gy2 * y2 );
        dnoise_dx += temp2 * x2;
        dnoise_dy += temp2 * y2;
        dnoise_dx *= -8.0f;
        dnoise_dy *= -8.0f;
        dnoise_dx += t40 * gx0 + t41 * gx1 + t42 * gx2;
        dnoise_dy += t40 * gy0 + t41 * gy1 + t42 * gy2;
        dnoise_dx *= 40.0f;
        dnoise_dy *= 40.0f;
		
	    return noise;
	  }
	
	const float F3 = 0.333333333f;
	const float G3 = 0.166666667f;
	
	public static float sdnoise3( float x, float y, float z,
	                out float dnoise_dx, out float dnoise_dy, out float dnoise_dz )
	{
	    float n0, n1, n2, n3;
	    float noise;
	    float gx0, gy0, gz0, gx1, gy1, gz1;
	    float gx2, gy2, gz2, gx3, gy3, gz3;
	
	    float s = (x+y+z)*F3;
	    float xs = x+s;
	    float ys = y+s;
	    float zs = z+s;
	    int i = FASTFLOOR(xs);
	    int j = FASTFLOOR(ys);
	    int k = FASTFLOOR(zs);
	
	    float t = (float)(i+j+k) * G3; 
	    float X0 = i-t;
	    float Y0 = j-t;
	    float Z0 = k-t;
	    float x0 = x-X0;
	    float y0 = y-Y0;
	    float z0 = z-Z0;
	
	    int i1, j1, k1;
	    int i2, j2, k2;
	
	    if(x0>=y0) {
	      if(y0>=z0)
	        { i1=1; j1=0; k1=0; i2=1; j2=1; k2=0; } /* X Y Z order */
	        else if(x0>=z0) { i1=1; j1=0; k1=0; i2=1; j2=0; k2=1; } /* X Z Y order */
	        else { i1=0; j1=0; k1=1; i2=1; j2=0; k2=1; } /* Z X Y order */
	      }
	    else { // x0<y0
	      if(y0<z0) { i1=0; j1=0; k1=1; i2=0; j2=1; k2=1; } /* Z Y X order */
	      else if(x0<z0) { i1=0; j1=1; k1=0; i2=0; j2=1; k2=1; } /* Y Z X order */
	      else { i1=0; j1=1; k1=0; i2=1; j2=1; k2=0; } /* Y X Z order */
	    }
	
	    float x1 = x0 - i1 + G3; 
	    float y1 = y0 - j1 + G3;
	    float z1 = z0 - k1 + G3;
	    float x2 = x0 - i2 + 2.0f * G3; 
	    float y2 = y0 - j2 + 2.0f * G3;
	    float z2 = z0 - k2 + 2.0f * G3;
	    float x3 = x0 - 1.0f + 3.0f * G3; 
	    float y3 = y0 - 1.0f + 3.0f * G3;
	    float z3 = z0 - 1.0f + 3.0f * G3;
	
	    int ii = i % 256;
	    int jj = j % 256;
	    int kk = k % 256;
	
	    float t0 = 0.6f - x0*x0 - y0*y0 - z0*z0;
	    float t20, t40;
	    if(t0 < 0.0f) n0 = t0 = t20 = t40 = gx0 = gy0 = gz0 = 0.0f;
	    else {
	      grad3(  hash(ii +  hash(jj +  hash(kk))), out gx0, out gy0, out gz0 );
	      t20 = t0 * t0;
	      t40 = t20 * t20;
	      n0 = t40 * ( gx0 * x0 + gy0 * y0 + gz0 * z0 );
	    }
	
	    float t1 = 0.6f - x1*x1 - y1*y1 - z1*z1;
	    float t21, t41;
	    if(t1 < 0.0f) n1 = t1 = t21 = t41 = gx1 = gy1 = gz1 = 0.0f;
	    else {
	      grad3(  hash(ii + i1 +  hash(jj + j1 +  hash(kk + k1))), out gx1, out gy1, out gz1 );
	      t21 = t1 * t1;
	      t41 = t21 * t21;
	      n1 = t41 * ( gx1 * x1 + gy1 * y1 + gz1 * z1 );
	    }
	
	    float t2 = 0.6f - x2*x2 - y2*y2 - z2*z2;
	    float t22, t42;
	    if(t2 < 0.0f) n2 = t2 = t22 = t42 = gx2 = gy2 = gz2 = 0.0f;
	    else {
	      grad3(  hash(ii + i2 +  hash(jj + j2 +  hash(kk + k2))), out gx2, out gy2, out gz2 );
	      t22 = t2 * t2;
	      t42 = t22 * t22;
	      n2 = t42 * ( gx2 * x2 + gy2 * y2 + gz2 * z2 );
	    }
	
	    float t3 = 0.6f - x3*x3 - y3*y3 - z3*z3;
	    float t23, t43;
	    if(t3 < 0.0f) n3 = t3 = t23 = t43 = gx3 = gy3 = gz3 = 0.0f;
	    else {
	      grad3(  hash(ii + 1 +  hash(jj + 1 +  hash(kk + 1))), out gx3, out gy3, out gz3 );
	      t23 = t3 * t3;
	      t43 = t23 * t23;
	      n3 = t43 * ( gx3 * x3 + gy3 * y3 + gz3 * z3 );
	    }
	
	    noise = 28.0f * (n0 + n1 + n2 + n3);
	
        float temp0 = t20 * t0 * ( gx0 * x0 + gy0 * y0 + gz0 * z0 );
        dnoise_dx = temp0 * x0;
        dnoise_dy = temp0 * y0;
        dnoise_dz = temp0 * z0;
        float temp1 = t21 * t1 * ( gx1 * x1 + gy1 * y1 + gz1 * z1 );
        dnoise_dx += temp1 * x1;
        dnoise_dy += temp1 * y1;
        dnoise_dz += temp1 * z1;
        float temp2 = t22 * t2 * ( gx2 * x2 + gy2 * y2 + gz2 * z2 );
        dnoise_dx += temp2 * x2;
        dnoise_dy += temp2 * y2;
        dnoise_dz += temp2 * z2;
        float temp3 = t23 * t3 * ( gx3 * x3 + gy3 * y3 + gz3 * z3 );
        dnoise_dx += temp3 * x3;
        dnoise_dy += temp3 * y3;
        dnoise_dz += temp3 * z3;
        dnoise_dx *= -8.0f;
        dnoise_dy *= -8.0f;
        dnoise_dz *= -8.0f;
        dnoise_dx += t40 * gx0 + t41 * gx1 + t42 * gx2 + t43 * gx3;
        dnoise_dy += t40 * gy0 + t41 * gy1 + t42 * gy2 + t43 * gy3;
        dnoise_dz += t40 * gz0 + t41 * gz1 + t42 * gz2 + t43 * gz3;
        dnoise_dx *= 28.0f;
        dnoise_dy *= 28.0f;
        dnoise_dz *= 28.0f;
		
	    return noise;
	}
	
	const float F4 = 0.309016994f;
	const float G4 = 0.138196601f;
	
	public static float sdnoise4( float x, float y, float z, float w,
	                out float dnoise_dx, out float dnoise_dy,
	                out float dnoise_dz, out float dnoise_dw)
	{
	    float n0, n1, n2, n3, n4; // Noise contributions from the five corners
	    float noise; // Return value
	    float gx0, gy0, gz0, gw0, gx1, gy1, gz1, gw1; /* Gradients at simplex corners */
	    float gx2, gy2, gz2, gw2, gx3, gy3, gz3, gw3, gx4, gy4, gz4, gw4;
	    float t20, t21, t22, t23, t24;
	    float t40, t41, t42, t43, t44;
	
	    // Skew the (x,y,z,w) space to determine which cell of 24 simplices we're in
	    float s = (x + y + z + w) * F4; // Factor for 4D skewing
	    float xs = x + s;
	    float ys = y + s;
	    float zs = z + s;
	    float ws = w + s;
	    int i = FASTFLOOR(xs);
	    int j = FASTFLOOR(ys);
	    int k = FASTFLOOR(zs);
	    int l = FASTFLOOR(ws);
	
	    float t = (i + j + k + l) * G4; // Factor for 4D unskewing
	    float X0 = i - t; // Unskew the cell origin back to (x,y,z,w) space
	    float Y0 = j - t;
	    float Z0 = k - t;
	    float W0 = l - t;
	
	    float x0 = x - X0;  // The x,y,z,w distances from the cell origin
	    float y0 = y - Y0;
	    float z0 = z - Z0;
	    float w0 = w - W0;
	
	    int c1 = (x0 > y0) ? 32 : 0;
	    int c2 = (x0 > z0) ? 16 : 0;
	    int c3 = (y0 > z0) ? 8 : 0;
	    int c4 = (x0 > w0) ? 4 : 0;
	    int c5 = (y0 > w0) ? 2 : 0;
	    int c6 = (z0 > w0) ? 1 : 0;
	    int c = c1 & c2 & c3 & c4 & c5 & c6;
	
	    int i1, j1, k1, l1;
	    int i2, j2, k2, l2;
	    int i3, j3, k3, l3;
	
	    i1 = simplex[c, 0]>=3 ? 1 : 0;
	    j1 = simplex[c, 1]>=3 ? 1 : 0;
	    k1 = simplex[c, 2]>=3 ? 1 : 0;
	    l1 = simplex[c, 3]>=3 ? 1 : 0;

	    i2 = simplex[c, 0]>=2 ? 1 : 0;
	    j2 = simplex[c, 1]>=2 ? 1 : 0;
	    k2 = simplex[c, 2]>=2 ? 1 : 0;
	    l2 = simplex[c, 3]>=2 ? 1 : 0;

	    i3 = simplex[c, 0]>=1 ? 1 : 0;
	    j3 = simplex[c, 1]>=1 ? 1 : 0;
	    k3 = simplex[c, 2]>=1 ? 1 : 0;
	    l3 = simplex[c, 3]>=1 ? 1 : 0;
	
	    float x1 = x0 - i1 + G4;
	    float y1 = y0 - j1 + G4;
	    float z1 = z0 - k1 + G4;
	    float w1 = w0 - l1 + G4;
	    float x2 = x0 - i2 + 2.0f * G4;
	    float y2 = y0 - j2 + 2.0f * G4;
	    float z2 = z0 - k2 + 2.0f * G4;
	    float w2 = w0 - l2 + 2.0f * G4;
	    float x3 = x0 - i3 + 3.0f * G4;
	    float y3 = y0 - j3 + 3.0f * G4;
	    float z3 = z0 - k3 + 3.0f * G4;
	    float w3 = w0 - l3 + 3.0f * G4;
	    float x4 = x0 - 1.0f + 4.0f * G4;
	    float y4 = y0 - 1.0f + 4.0f * G4;
	    float z4 = z0 - 1.0f + 4.0f * G4;
	    float w4 = w0 - 1.0f + 4.0f * G4;
	
	    int ii = i & 0xff;
	    int jj = j & 0xff;
	    int kk = k & 0xff;
	    int ll = l & 0xff;
	
	    float t0 = 0.6f - x0*x0 - y0*y0 - z0*z0 - w0*w0;
	    if(t0 < 0.0f) n0 = t0 = t20 = t40 = gx0 = gy0 = gz0 = gw0 = 0.0f;
	    else {
	      t20 = t0 * t0;
	      t40 = t20 * t20;
	      grad4( hash(ii+ hash(jj+ hash(kk+ hash(ll)))), out gx0, out gy0, out gz0, out gw0);
	      n0 = t40 * ( gx0 * x0 + gy0 * y0 + gz0 * z0 + gw0 * w0 );
	    }
	
	   float t1 = 0.6f - x1*x1 - y1*y1 - z1*z1 - w1*w1;
	    if(t1 < 0.0f) n1 = t1 = t21 = t41 = gx1 = gy1 = gz1 = gw1 = 0.0f;
	    else {
	      t21 = t1 * t1;
	      t41 = t21 * t21;
	      grad4( hash(ii+i1+ hash(jj+j1+ hash(kk+k1+ hash(ll+l1)))), out gx1, out gy1, out gz1, out gw1);
	      n1 = t41 * ( gx1 * x1 + gy1 * y1 + gz1 * z1 + gw1 * w1 );
	    }
	
	   float t2 = 0.6f - x2*x2 - y2*y2 - z2*z2 - w2*w2;
	    if(t2 < 0.0f) n2 = t2 = t22 = t42 = gx2 = gy2 = gz2 = gw2 = 0.0f;
	    else {
	      t22 = t2 * t2;
	      t42 = t22 * t22;
	      grad4( hash(ii+i2+ hash(jj+j2+ hash(kk+k2+ hash(ll+l2)))), out gx2, out gy2, out gz2, out gw2);
	      n2 = t42 * ( gx2 * x2 + gy2 * y2 + gz2 * z2 + gw2 * w2 );
	   }
	
	   float t3 = 0.6f - x3*x3 - y3*y3 - z3*z3 - w3*w3;
	    if(t3 < 0.0f) n3 = t3 = t23 = t43 = gx3 = gy3 = gz3 = gw3 = 0.0f;
	    else {
	      t23 = t3 * t3;
	      t43 = t23 * t23;
	      grad4( hash(ii+i3+ hash(jj+j3+ hash(kk+k3+ hash(ll+l3)))), out gx3, out gy3, out gz3, out gw3);
	      n3 = t43 * ( gx3 * x3 + gy3 * y3 + gz3 * z3 + gw3 * w3 );
	    }
	
	   float t4 = 0.6f - x4*x4 - y4*y4 - z4*z4 - w4*w4;
	    if(t4 < 0.0f) n4 = t4 = t24 = t44 = gx4 = gy4 = gz4 = gw4 = 0.0f;
	    else {
	      t24 = t4 * t4;
	      t44 = t24 * t24;
	      grad4( hash(ii+1+ hash(jj+1+ hash(kk+1+ hash(ll+1)))), out gx4, out gy4, out gz4, out gw4);
	      n4 = t44 * ( gx4 * x4 + gy4 * y4 + gz4 * z4 + gw4 * w4 );
	    }
	
	    noise = 27.0f * (n0 + n1 + n2 + n3 + n4);
		
        float temp0 = t20 * t0 * ( gx0 * x0 + gy0 * y0 + gz0 * z0 + gw0 * w0 );
        dnoise_dx = temp0 * x0;
        dnoise_dy = temp0 * y0;
        dnoise_dz = temp0 * z0;
        dnoise_dw = temp0 * w0;
        float temp1 = t21 * t1 * ( gx1 * x1 + gy1 * y1 + gz1 * z1 + gw1 * w1 );
        dnoise_dx += temp1 * x1;
        dnoise_dy += temp1 * y1;
        dnoise_dz += temp1 * z1;
        dnoise_dw += temp1 * w1;
        float temp2 = t22 * t2 * ( gx2 * x2 + gy2 * y2 + gz2 * z2 + gw2 * w2 );
        dnoise_dx += temp2 * x2;
        dnoise_dy += temp2 * y2;
        dnoise_dz += temp2 * z2;
        dnoise_dw += temp2 * w2;
        float temp3 = t23 * t3 * ( gx3 * x3 + gy3 * y3 + gz3 * z3 + gw3 * w3 );
        dnoise_dx += temp3 * x3;
        dnoise_dy += temp3 * y3;
        dnoise_dz += temp3 * z3;
        dnoise_dw += temp3 * w3;
        float temp4 = t24 * t4 * ( gx4 * x4 + gy4 * y4 + gz4 * z4 + gw4 * w4 );
        dnoise_dx += temp4 * x4;
        dnoise_dy += temp4 * y4;
        dnoise_dz += temp4 * z4;
        dnoise_dw += temp4 * w4;
        dnoise_dx *= -8.0f;
        dnoise_dy *= -8.0f;
        dnoise_dz *= -8.0f;
        dnoise_dw *= -8.0f;
        dnoise_dx += t40 * gx0 + t41 * gx1 + t42 * gx2 + t43 * gx3 + t44 * gx4;
        dnoise_dy += t40 * gy0 + t41 * gy1 + t42 * gy2 + t43 * gy3 + t44 * gy4;
        dnoise_dz += t40 * gz0 + t41 * gz1 + t42 * gz2 + t43 * gz3 + t44 * gz4;
        dnoise_dw += t40 * gw0 + t41 * gw1 + t42 * gw2 + t43 * gw3 + t44 * gw4;

        dnoise_dx *= 27.0f;
        dnoise_dy *= 27.0f;
        dnoise_dz *= 27.0f;
        dnoise_dw *= 27.0f;

	    return noise;
	}
}
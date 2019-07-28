  __kernel void BitmapDistances(
      __global int* sample, __global int* refImgs, __global int* retDif,
	    const int retDifSize, const int samplesSize,
	    const int stride, const int refImgSize)
    {
	     int maxValue = (retDifSize/samplesSize)/sizeof(int)/sizeof(int);
       int globalId0 = get_global_id(0);
       int globalId1 = get_global_id(1);
       int groupId0 = get_group_id(0);
       int groupId1 = get_group_id(1);
       int localId0 = get_local_id(0);
       int localId1 = get_local_id(1);
       //printf("globalId: (%d, %d); groupId: (%d, %d); localId: (%d, %d)\n", globalId0, globalId1, groupId0, groupId1, localId0, localId1);
	     int globalInd = globalId0 * stride + globalId1;

	     if (globalInd >= maxValue)
		       return;
       globalInd *= 4;

       for (int i = 0; i < samplesSize; i++)
       {
         int sampleInd = globalInd % (refImgSize) + i * refImgSize;
		 unsigned int val1 = (unsigned int)*(sample + (sampleInd/4));
		 unsigned int val2 = (unsigned int)*(refImgs + (globalInd/4));

         int mask = 0xFF;
         *(retDif + globalInd + 0 + i * ((retDifSize/samplesSize)/sizeof(int))) = ((val1 & mask) - (val2 & mask)) * ((val1 & mask) - (val2 & mask));

         mask = mask << 8;
         *(retDif + globalInd + 1 + i * ((retDifSize/samplesSize)/sizeof(int))) = (((val1 & mask) >> 8) - ((val2 & mask) >> 8)) * (((val1 & mask) >> 8) - ((val2 & mask) >> 8));

         mask = mask << 8;
         *(retDif + globalInd + 2 + i * ((retDifSize/samplesSize)/sizeof(int))) = (((val1 & mask) >> 16) - ((val2 & mask) >> 16)) * (((val1 & mask) >> 16) - ((val2 & mask) >> 16));

         mask = mask << 8;
         *(retDif + globalInd + 3 + i * ((retDifSize/samplesSize)/sizeof(int))) = (((val1 & mask) >> 24) - ((val2 & mask) >> 24)) * (((val1 & mask) >> 24) - ((val2 & mask) >> 24));
       }
    }

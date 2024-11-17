#pragma once
#include "hls_stream.h"
#include "ap_axi_sdata.h"
#include "ap_fixed.h"
#include <hls_math.h>

typedef ap_fixed<32,16> fixed_ft;

typedef struct {
	int data;
	bool last;
} data_t;


typedef union {
	int intdata;
	float floatdata;
} data_itoft;

void conv1_matmul(float a[360], float b[864]);
void conv2_matmul(float a[432], float b[736]);
void conv3_matmul(float a[352], float b[576]);
void fc1_matmul(float a[65], float b[65]);
void fc2_matmul(float a[65], float b[8]);
void max_pool(float* a, float* b, int N, int step_size);
void global_pool(float a[256], float b[64]);

void soccer_conv1_matmul(float a[360], float b[864]);
void soccer_conv2_matmul(float a[432], float b[736]);
void soccer_conv3_matmul(float a[352], float b[576]);
void soccer_fc1_matmul(float a[65], float b[65]);
void soccer_fc2_matmul(float a[65], float b[2]);
void relu(float a[64]);

void CNN(hls::stream<data_t> &input_stream, hls::stream<data_t> &output_stream);

#include "components.h"
#include <cfloat>

void CNN(hls::stream<data_t> &input_stream, hls::stream<data_t> &output_stream) {
	#pragma HLS INTERFACE ap_ctrl_none port=return
	#pragma HLS INTERFACE axis port=input_stream
	#pragma HLS INTERFACE axis port=output_stream

	float input_buffer[361];
	float output_buffer[8];

	data_t temp1;
	data_itoft data_translator;

	for (int i = 0; i < 361; ++i) {
    	#pragma HLS PIPELINE II=1
		temp1 = input_stream.read();
		data_translator.intdata = temp1.data;
		input_buffer[i] = data_translator.floatdata;
	}

	float first_buffer1[864] = {0};
	float first_buffer2[432] = {-FLT_MAX};

	float second_buffer1[736] = {0};
	float second_buffer2[352] = {-FLT_MAX};
	float third_buffer1[576] = {0};
	float third_buffer2[256] = {-FLT_MAX};
	float fourth_buffer[65] = {0};
	float fifth_buffer[65];

	if(input_buffer[0] == 0) {
		conv1_matmul(input_buffer + 1, first_buffer1);
		max_pool(first_buffer1, first_buffer2, 27, 16);

		conv2_matmul(first_buffer2, second_buffer1);
		max_pool(second_buffer1, second_buffer2, 11, 32);


		conv3_matmul(second_buffer2, third_buffer1);
		max_pool(third_buffer1, third_buffer2, 4, 64);

		float fourth_buffer[65] = {0};
		global_pool(third_buffer2, fourth_buffer);
		fourth_buffer[64] = 1;

		float fifth_buffer[65];
		fc1_matmul(fourth_buffer, fifth_buffer);
		relu(fifth_buffer);

		fc2_matmul(fifth_buffer, output_buffer);
	} else {
		soccer_conv1_matmul(input_buffer + 1, first_buffer1);
		max_pool(first_buffer1, first_buffer2, 27, 16);

		soccer_conv2_matmul(first_buffer2, second_buffer1);
		max_pool(second_buffer1, second_buffer2, 11, 32);

		soccer_conv3_matmul(second_buffer2, third_buffer1);
		max_pool(third_buffer1, third_buffer2, 4, 64);

		float fourth_buffer[65] = {0};
		global_pool(third_buffer2, fourth_buffer);
		fourth_buffer[64] = 1;

		float fifth_buffer[65];
		soccer_fc1_matmul(fourth_buffer, fifth_buffer);
		relu(fifth_buffer);

		soccer_fc2_matmul(fifth_buffer, output_buffer);
	}

	data_t temp2;
	for (int i = 0; i < 8; ++i) {
		data_translator.floatdata = output_buffer[i];
		temp2.data = data_translator.intdata;
		temp2.last = (i == 8 - 1) ? 1 : 0;
		output_stream.write(temp2);
	}
}






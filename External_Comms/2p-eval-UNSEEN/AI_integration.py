# from pynq import allocate, Overlay
# import numpy as np
# import pandas as pd
# import json
# import time
# from scipy.stats import skew, kurtosis

# class AI():
#     def __init__(self):
#         self.overlay = Overlay("/home/xilinx/AI/nonnull.bit")
#         dma = self.overlay.axi_dma_0
#         self.dma_send = dma.sendchannel
#         self.dma_recv = dma.recvchannel


#     def fpga_comm(self, X):
#         input_buffer = allocate(shape=(361,), dtype=np.float32)
#         output_buffer = allocate(shape=(7,), dtype=np.float32)
#         np.copyto(input_buffer, X)
#         self.dma_send.transfer(input_buffer)
#         self.dma_recv.transfer(output_buffer)
#         self.dma_send.wait()
#         self.dma_recv.wait()
#         return output_buffer

#     def inference(self, data, action):
#         rows = []
#         for x in data:
#             features = x['feature']
#             features = [eval(x) for x in features]
#             rows.append(features)
#         rows = np.array(rows)
#         flattened_rows = rows.flatten()
#         flattened_rows = np.pad(flattened_rows, (0, 360 - len(flattened_rows)), 'constant')
#         X = np.zeros((361,))
#         if (action == "glove"): 
#             X[0] = 0
#         else:
#             X[0] = 1
#         np.copyto(X[1:], flattened_rows)
#         result = self.fpga_comm(X)

#         glove_map = {
#             0: "null",
#             1: "reload",
#             2: "bomb",
#             3: "shield",
#             4: "basket",
#             5: "bowl",
#             6: "volley",
#             7: "logout"
#         }
#         soccer_map = {
#             0: "soccer",
#             1: "null"
#         }

#         output = {}
#         output['player_id'] = data[0]['player_id']
#         if (action == "glove"):
#             action_code = np.argmax(result) + 1
#             output['action'] = glove_map.get(action_code, "null")
            
#         elif (action == "soccer"):
#             action_code = np.argmax(result[:2])
#             output['action'] = soccer_map.get(action_code, "null")
            
#         output = json.dumps(output)
#         print(result)
#         # print(percentage)
#         return output


from pynq import allocate, Overlay
import numpy as np
import pandas as pd
import json
import time
from scipy.stats import skew, kurtosis

class AI():
    def __init__(self):
        self.overlay = Overlay("/home/xilinx/AI/null.bit")
        dma = self.overlay.axi_dma_0
        self.dma_send = dma.sendchannel
        self.dma_recv = dma.recvchannel


    def fpga_comm(self, X):
        input_buffer = allocate(shape=(361,), dtype=np.float32)
        output_buffer = allocate(shape=(8,), dtype=np.float32)
        np.copyto(input_buffer, X)
        self.dma_send.transfer(input_buffer)
        self.dma_recv.transfer(output_buffer)
        self.dma_send.wait()
        self.dma_recv.wait()
        return output_buffer

    def inference(self, data, action):
        rows = []
        for x in data:
            features = x['feature']
            features = [eval(x) for x in features]
            rows.append(features)
        rows = np.array(rows)
        flattened_rows = rows.flatten()
        flattened_rows = np.pad(flattened_rows, (0, 360 - len(flattened_rows)), 'constant')
        X = np.zeros((361,))
        if (action == "glove"): 
            X[0] = 0
        else:
            X[0] = 1
        np.copyto(X[1:], flattened_rows)
        result = self.fpga_comm(X)

        glove_map = {
            0: "null",
            1: "reload",
            2: "bomb",
            3: "shield",
            4: "basket",
            5: "bowl",
            6: "volley",
            7: "logout"
        }
        soccer_map = {
            0: "soccer",
            1: "null"
        }

        output = {}
        output['player_id'] = data[0]['player_id']
        if (action == "glove"):
            action_code = np.argmax(result)
            output['action'] = glove_map.get(action_code, "null")
            
        elif (action == "soccer"):
            # action_code = np.argmax(result[:2])
            # output['action'] = soccer_map.get(action_code, "null")
            if (result[0] - result[1] > 10):
                output['action'] = "soccer"
            else:
                output['action'] = "null"
        output = json.dumps(output)
        print(result)
        # print(percentage)
        return output


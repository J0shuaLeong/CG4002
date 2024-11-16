from pynq import allocate, Overlay
import numpy as np
import pandas as pd
import json
import time

def mad(series):
    return np.median(np.abs(series - np.median(series)))

class AI():
    def __init__(self):
        self.overlay = Overlay("/home/xilinx/AI/bomb.bit")
        dma = self.overlay.axi_dma_0
        self.dma_send = dma.sendchannel
        self.dma_recv = dma.recvchannel

        self.train_mean = [1308.9935297220638, 550.8239648118199, 223.34372347546133, 12.884607736292256, 24.81507669586439, -4.537836014071107, 1012.912811095944, 543.306269361239, 1881.802341119419, 549.5803264261383, 1015.7639940443917, 695.7138392265047, 821.541637537479, 222.24921080141, 459.2896651063725, 325.0452586237904, 989.9358104628137, 241.34512253338607, 1100.62366247687, 341.60112232838077, -141.41589958158997, -859.2644351464435, -1614.7907949790795, -494.5121338912134, -758.5882845188285, -646.2092050209205, 18.410878661087867, 165.18075313807532, 498.4482850328352, 60.33023352939359, 3983.73640167364, 2195.1138075313806, 2016.3740585774058, 485.2384937238494, 1054.5405857740586, 650.4502092050209, 3797.726359832636, 1029.1690376569038, 4803.7637508444495, 1335.7984642159242, 528.6510460251046, 386.7008368200837, 429.2205020920502, 140.4518828451883, 271.52008368200836, 187.30167364016737, 453.3, 177.418410041841, 589.7949358847422, 228.6874372879791]

        self.train_std = [539.5325834780629, 233.13596486102244, 548.7037341175287, 132.7949324908741, 64.25754825066814, 65.07441246660392, 524.3517956130925, 150.19566998818175, 557.5546986295478, 161.53248181566502, 479.3750435557769, 189.99068855496142, 381.3684324272767, 80.45050650621677, 223.49375629791862, 91.76247166780881, 499.5555172368493, 65.75070016756351, 482.8044294472342, 88.60653057774704, 546.4227831907095, 862.2547306049544, 1114.8230974733335, 249.63066705925917, 334.25138165592995, 226.0801089160326, 30.792207602228682, 98.20111273143597, 239.77543371224584, 45.873750636897775, 1686.4080656794565, 711.6562498918905, 1261.7451518753767, 303.0949766625403, 523.6000396207684, 248.6412410838931, 1776.9270789623054, 258.76641355075253, 1782.2528816908048, 369.55573834039615, 304.2886182037736, 150.69597757141196, 270.47744961680695, 74.60244133925558, 209.04336904235632, 79.23349845572677, 290.83204717718434, 69.14297415109853, 302.78278090716094, 86.47197569118065]

    def inference(self, data):
        rows = []
        for x in data:
            features = x['feature']
            features = [eval(x) for x in features]
            rows.append(features)
        rows = np.array(rows)

        acc_magnitude = np.sqrt(np.sum(rows[:, :3]**2, axis=1))
        gyr_magnitude = np.sqrt(np.sum(rows[:, 3:6]**2, axis=1))
        rows = np.column_stack((rows, acc_magnitude, gyr_magnitude))

        X = [np.mean(rows, axis=0), np.std(rows, axis=0), np.min(rows, axis=0), 
             np.max(rows, axis=0), np.apply_along_axis(mad, axis=0, arr=rows)]   
        X = np.array(X).flatten()
        X = (X - self.train_mean) / self.train_std

        input_buffer = allocate(shape=(50,), dtype=np.float32)
        output_buffer = allocate(shape=(1,), dtype=int)
        np.copyto(input_buffer, X)
        self.dma_send.transfer(input_buffer)
        self.dma_recv.transfer(output_buffer)
        self.dma_send.wait()
        self.dma_recv.wait()

        # Map output_buffer[0] to action
        action_map = {
            0: "null",
            1: "reload",
            2: "bomb",
            3: "shield",
            4: "basket",
            5: "bowl",
            6: "volley",
            7: "logout",
            # 7: "soccer"
        }

        output = {}
        output['player_id'] = data[0]['player_id']
        action_code = int(output_buffer[0])
        output['action'] = action_map.get(action_code, "null")
        output = json.dumps(output)

        return output



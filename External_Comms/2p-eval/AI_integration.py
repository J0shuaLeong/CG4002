from pynq import allocate, Overlay
import numpy as np
import pandas as pd
import json
import time
from scipy.stats import skew, kurtosis

def mad(series):
    return np.median(np.abs(series - np.median(series)))

def freq_skewness(series):
    fft_result = np.fft.fft(series)
    fft_magnitude = np.abs(fft_result)  # Magnitude of the FFT
    return skew(fft_magnitude)

def freq_kurtosis(series):
    fft_result = np.fft.fft(series)
    fft_magnitude = np.abs(fft_result)  # Magnitude of the FFT

    return kurtosis(fft_magnitude)


def median_freq(series):
    fft_result = np.fft.fft(series)
    fft_magnitude = np.abs(fft_result)  # Magnitude of the FFT

    return np.median(fft_magnitude)

class AI():
    def __init__(self):
        self.overlay = Overlay("/home/xilinx/AI/051124.bit")
        dma = self.overlay.axi_dma_0
        self.dma_send = dma.sendchannel
        self.dma_recv = dma.recvchannel
        self.train_mean = [1036.4760755563057, 421.6960118330857, 500.63805064770594, -27.906426304397243, 7.447188084706297, 3.9918124370719443, 1716.0539671821464, 450.6912313521556, 732.8775862623338, 722.4862602439689, 823.393095536971, 225.5723578932441, 370.3590352300652, 276.17268086181167, 921.9778233243477, 321.4944392537154, 946.3896128784975, 333.0036412418551, 415.0122652357225, -14.002683020314297, -7.96780375622844, -6.153123802223074, 1491.7574162526562, 384.1418963474395, 420.5555768493676, 356.44269835185895, 423.26101954771946, 116.80778075891146, 209.09984668455346, 131.58968953622076, 509.3881741825373, 202.08166303803156, 1810.4942934390442, 2092.1073261936735, 2075.196124474207, 653.4541472591711, 993.4841974396159, 663.4918024022755, 2098.690445149322, 801.9159405068481, 5.242850293463889, 2.995195832702604, 3.2782862569678617, 2.3048103124140003, 2.2316171199528005, 2.375150349218263, 6.098690732001813, 5.150633656701188, 32.04837031669002, 11.770759521051184, 13.84923281008081, 5.620189853943816, 5.069400962522371, 5.332153992605268, 40.690082235669216, 30.588013092094357]
        self.train_std = [330.39317622231613, 252.50171824694223, 629.1050941026692, 64.75053300255081, 60.53503229322335, 55.61481743427674, 440.85539075006193, 168.22543354927063, 310.5526188867252, 267.6805336501721, 368.98889186494415, 74.24440759535462, 248.4419862906357, 75.06390955669258, 326.23557040408093, 103.45434369662237, 286.28960831114466, 216.9130681094991, 595.0729539060665, 57.74450898423285, 66.21354206464463, 42.3713328723949, 424.64675021671724, 198.41825618498632, 293.3596826232116, 188.76641431515606, 348.2545618383181, 58.81164870825581, 203.68809929342268, 65.40243088565285, 332.64391135764055, 94.57245021770841, 940.5019895904826, 908.7947824692038, 1252.8784341044918, 310.99359866416177, 585.0148910947922, 282.9824986045325, 912.4829598461216, 405.32418530787476, 1.3157813809724694, 1.1741928304508988, 1.2624325663287734, 0.672543916944117, 0.7908786509261128, 0.5335306365823784, 0.5139686685371275, 0.9143518292065724, 13.237666357443588, 9.610417994118043, 10.676832025527524, 3.927945339840311, 4.005180263137134, 3.071966627334473, 5.561840265911561, 9.873154302571848]

    def extract_features(self, data):
        X = [np.mean(data, axis=0), np.std(data, axis=0), np.median(data, axis = 0), np.apply_along_axis(mad, axis=0, arr=data),
             np.apply_along_axis(median_freq, axis=0, arr=data), np.apply_along_axis(freq_skewness, axis=0, arr=data), 
             np.apply_along_axis(freq_kurtosis, axis=0, arr=data)]
           
        X = np.array(X).flatten()
        X = (X - self.train_mean) / self.train_std
        X = np.append(X, 1)
        return X
    
    def fpga_comm(self, X):
        input_buffer = allocate(shape=(57,), dtype=np.float32)
        output_buffer = allocate(shape=(1,), dtype=int)
        np.copyto(input_buffer, X)
        self.dma_send.transfer(input_buffer)
        self.dma_recv.transfer(output_buffer)
        self.dma_send.wait()
        self.dma_recv.wait()
        return output_buffer[0]

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

        X = self.extract_features(rows)

        result = self.fpga_comm(X) + 1
        # input_buffer = allocate(shape=(49,), dtype=np.float32)
        # output_buffer = allocate(shape=(8,), dtype=np.float32)
        # np.copyto(input_buffer, X_30)
        # self.dma_send.transfer(input_buffer)
        # self.dma_recv.transfer(output_buffer)
        # self.dma_send.wait()
        # self.dma_recv.wait()

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
            # 8: "soccer"
        }

        output = {}
        output['player_id'] = data[0]['player_id']
#action_code = int(output_buffer[0])
        action_code = result
        output['action'] = action_map.get(action_code, "null")
        output = json.dumps(output)

        return output



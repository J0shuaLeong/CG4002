

class ConsoleColors:
    RED = '\033[91m'        # for system shut down
    GREEN = '\033[92m'      # for system start up
    YELLOW = '\033[93m'     # from eval server
    VIS1 = '\033[94m'       # vis 1
    VIS2 = '\033[95m'    # vis 2
    
    AIOUTPUT = '\033[1;42m'     # AI action out put

    DEVICE1 = '\033[96m'       # device 1 - CYAN - P1 HAND
    DEVICE4 = '\033[38;5;214m' # device 4 - ORANGE - P1 LEG
    DEVICE5 = '\033[38;5;213m' # device 5 - PINK - P2 HAND
    DEVICE8 = '\033[38;5;119m' # device 8 LIGHT_GREEN - P2 LEG
    RESET = '\033[0m'       

import torch
from torch import nn
from torch.nn import functional as F

class CNN(nn.Module):
    def __init__(self, num_classes, dropout_rate):
        super(CNN, self).__init__()
        self.conv1 = nn.Conv1d(6, 16, 7)
        self.conv2 = nn.Conv1d(16, 32, 5)
        self.conv3 = nn.Conv1d(32, 64, 3)
        
        self.fc1 = nn.Linear(64,64)
        self.fc2 = nn.Linear(64, num_classes)
        
        self.pool = nn.MaxPool1d(2)
        self.dropout = nn.Dropout(dropout_rate)
        
    def forward(self, x):
        x = F.relu(self.conv1(x))
        
        x = self.pool(x)
        
        x = F.relu(self.conv2(x))
        x = self.pool(x)
        
        x = F.relu(self.conv3(x))
        x = self.pool(x)
        x = x.mean(dim=2)
        
        x = F.relu(self.fc1(x))
        x = self.dropout(x)
        
        x = self.fc2(x)
        
        return x

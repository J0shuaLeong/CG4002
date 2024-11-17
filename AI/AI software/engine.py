import torch
from torch import nn
import matplotlib.pyplot as plt
from sklearn.metrics import confusion_matrix, ConfusionMatrixDisplay, classification_report


def train(model, train_loader, criterion, optimizer, device, num_epochs=10):
    for epoch in range(num_epochs + 1):
        total_loss = 0
        model.train()
        for i, data in enumerate(train_loader):
            data = data.to(device).float()
            optimizer.zero_grad()
            output = model(data[:, :6,:])
            loss = criterion(output, data[:, 6, 0].long())
            loss.backward()
            optimizer.step()
            total_loss += loss.item()
        # if epoch % 10 == 0:
        #     print('Epoch:', epoch, 'Loss:', total_loss)
    print('Loss:', total_loss)

def evaluate(model, test_loader, device):
    with torch.no_grad():
        model.eval()
        correct = 0
        total = 0
        all_preds = []
        all_labels = []
        
        for i, data in enumerate(test_loader):
            data = data.to(device).float()

            output = model(data[:, :6, :])
            _, predicted = torch.max(output, 1)
            
            all_preds.extend(predicted.cpu().numpy())
            all_labels.extend(data[:, 6, 0].cpu().numpy())
            
            total += data.size(0)
            correct += (predicted == data[:, 6, 0].long()).sum().item()

        accuracy = correct / total * 100
        print(f"Accuracy: {accuracy:.2f}%")
        return all_labels, all_preds, accuracy

def plot_confusion_matrix(y, y_pred, acc):
    matrix = confusion_matrix(y, y_pred)
    fig, ax = plt.subplots(figsize=(10, 10))
    disp = ConfusionMatrixDisplay(confusion_matrix=matrix)
    disp.plot(ax=ax)
    plt.title(f"Confusion Matrix\nAccuracy: {acc:.2f}%")
    plt.show()

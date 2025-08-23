import React, { useState, useRef } from 'react';
import { Button } from './ui/button';
import { Dialog, DialogContent, DialogHeader, DialogTitle } from './ui/dialog';
import { Printer, Download, X } from 'lucide-react';
import { toast } from 'sonner';

interface BrowserReceiptData {
  receiptData: any;
  template: any;
  htmlTemplate: string;
  cssStyles: string;
  transactionNumber: string;
  transactionDate: string;
  transactionType: string;
}

interface ReceiptPrinterProps {
  isOpen: boolean;
  onClose: () => void;
  transactionId?: number;
  receiptData?: BrowserReceiptData;
  onGenerateReceipt?: (transactionId: number) => Promise<BrowserReceiptData>;
}

const ReceiptPrinter: React.FC<ReceiptPrinterProps> = ({
  isOpen,
  onClose,
  transactionId,
  receiptData,
  onGenerateReceipt
}) => {
  const [isLoading, setIsLoading] = useState(false);
  const [currentReceiptData, setCurrentReceiptData] = useState<BrowserReceiptData | null>(receiptData || null);

  const handleGenerateReceipt = async () => {
    if (!transactionId || !onGenerateReceipt) {
      toast.error('Transaction ID is required');
      return;
    }

    setIsLoading(true);
    try {
      const data = await onGenerateReceipt(transactionId);
      setCurrentReceiptData(data);
      toast.success('Receipt generated successfully');
    } catch (error) {
      console.error('Error generating receipt:', error);
      toast.error('Failed to generate receipt');
    } finally {
      setIsLoading(false);
    }
  };

  const handlePrint = () => {
    if (!currentReceiptData) {
      toast.error('No receipt data available');
      return;
    }

    try {
      const printWindow = window.open('', '_blank');
      if (!printWindow) {
        toast.error('Popup blocked. Please allow popups for this site.');
        return;
      }

      printWindow.document.write(currentReceiptData.htmlTemplate);
      printWindow.document.close();

      printWindow.onload = () => {
        printWindow.print();
        printWindow.close();
      };

      toast.success('Print dialog opened');
    } catch (error) {
      console.error('Error printing receipt:', error);
      toast.error('Failed to print receipt');
    }
  };

  const handleDownload = () => {
    if (!currentReceiptData) {
      toast.error('No receipt data available');
      return;
    }

    try {
      const blob = new Blob([currentReceiptData.htmlTemplate], { type: 'text/html' });
      const url = URL.createObjectURL(blob);
      
      const link = document.createElement('a');
      link.href = url;
      link.download = `receipt_${currentReceiptData.transactionNumber}_${new Date().toISOString().split('T')[0]}.html`;
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
      
      URL.revokeObjectURL(url);
      
      toast.success('Receipt downloaded successfully');
    } catch (error) {
      console.error('Error downloading receipt:', error);
      toast.error('Failed to download receipt');
    }
  };

  return (
    <Dialog open={isOpen} onOpenChange={onClose}>
      <DialogContent className="max-w-4xl max-h-[90vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle className="flex items-center justify-between">
            <span>Receipt Printer</span>
            <Button variant="ghost" size="sm" onClick={onClose}>
              <X className="h-4 w-4" />
            </Button>
          </DialogTitle>
        </DialogHeader>

        <div className="space-y-4">
          {!currentReceiptData && transactionId && (
            <div className="flex justify-center">
              <Button 
                onClick={handleGenerateReceipt} 
                disabled={isLoading}
                className="flex items-center gap-2"
              >
                {isLoading ? 'Generating...' : 'Generate Receipt'}
              </Button>
            </div>
          )}

          {currentReceiptData && (
            <>
              <div className="flex justify-end gap-2">
                <Button 
                  onClick={handlePrint}
                  className="flex items-center gap-2"
                  variant="outline"
                >
                  <Printer className="h-4 w-4" />
                  Print Receipt
                </Button>
                <Button 
                  onClick={handleDownload}
                  className="flex items-center gap-2"
                  variant="outline"
                >
                  <Download className="h-4 w-4" />
                  Download HTML
                </Button>
              </div>

              <div className="border rounded-lg p-4 bg-white">
                <div 
                  className="receipt-preview max-w-md mx-auto"
                  dangerouslySetInnerHTML={{ __html: currentReceiptData.htmlTemplate }}
                />
              </div>
            </>
          )}
        </div>
      </DialogContent>
    </Dialog>
  );
};

export default ReceiptPrinter;

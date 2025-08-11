import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';

export interface ApiResponse<T> { data: T; success: boolean; message?: string; }
export interface ProductDto { id: number; productCode: string; name: string; karatType: number; weight: number; }

@Injectable({ providedIn: 'root' })
export class LabelPrintingService {
  private baseUrl = environment.apiUrl + '/labels';

  constructor(private http: HttpClient) {}

  printProductLabel(productId: number, copies = 1) {
    return this.http.post<ApiResponse<string>>(`${this.baseUrl}/${productId}/print?copies=${copies}`, {});
  }

  generateProductZpl(productId: number, copies = 1) {
    return this.http.get<ApiResponse<string>>(`${this.baseUrl}/${productId}/zpl?copies=${copies}`);
  }

  decodeQr(payload: string) {
    return this.http.post<ApiResponse<ProductDto>>(`${this.baseUrl}/decode-qr`, { payload });
  }
}



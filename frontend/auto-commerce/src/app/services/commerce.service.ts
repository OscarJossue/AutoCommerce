import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class CommerceService {

  private readonly apiUrl = 'http://localhost:5294/api/commerce';

  constructor(private http: HttpClient) {}

  getDatabaseStatus(): Observable<any> {
    return this.http.get(`${this.apiUrl}/database-status`);
  }

  uploadCsv(file: File): Observable<any> {
    const formData = new FormData();

    formData.append('file', file);

    return this.http.post(
      `${this.apiUrl}/upload`,
      formData
    );
  }

  processCommerce(processDate: string): Observable<any> {
    return this.http.post(
      `${this.apiUrl}/process`,
      {
        processDate
      }
    );
  }

  getQuarantine(): Observable<any> {
    return this.http.get(
      `${this.apiUrl}/quarantine`
    );
  }
}
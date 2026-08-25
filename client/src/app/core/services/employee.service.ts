import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  CreateDeveloperRequest,
  CreateManagerRequest,
  DeveloperDto,
  EmployeeDto,
  EmployeeSearchRequest,
  ManagerDto,
  UpdateDeveloperDetailsRequest,
  UpdateEmployeeRequest,
  UpdateManagerDetailsRequest
} from '../models/employee.model';
import { PagedResult } from '../models/paged-result.model';

@Injectable({ providedIn: 'root' })
export class EmployeeService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/employees`;

  search(request: EmployeeSearchRequest): Observable<PagedResult<EmployeeDto>> {
    let params = new HttpParams();
    Object.entries(request).forEach(([key, value]) => {
      if (value !== null && value !== undefined && value !== '') {
        params = params.set(key, String(value));
      }
    });
    return this.http.get<PagedResult<EmployeeDto>>(this.baseUrl, { params });
  }

  getById(id: number): Observable<EmployeeDto> {
    return this.http.get<EmployeeDto>(`${this.baseUrl}/${id}`);
  }

  createDeveloper(request: CreateDeveloperRequest): Observable<DeveloperDto> {
    return this.http.post<DeveloperDto>(`${this.baseUrl}/developers`, request);
  }

  createManager(request: CreateManagerRequest): Observable<ManagerDto> {
    return this.http.post<ManagerDto>(`${this.baseUrl}/managers`, request);
  }

  update(id: number, request: UpdateEmployeeRequest): Observable<EmployeeDto> {
    return this.http.put<EmployeeDto>(`${this.baseUrl}/${id}`, request);
  }

  updateDeveloperDetails(id: number, request: UpdateDeveloperDetailsRequest): Observable<DeveloperDto> {
    return this.http.patch<DeveloperDto>(`${this.baseUrl}/developers/${id}`, request);
  }

  updateManagerDetails(id: number, request: UpdateManagerDetailsRequest): Observable<ManagerDto> {
    return this.http.patch<ManagerDto>(`${this.baseUrl}/managers/${id}`, request);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}

import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatListModule } from '@angular/material/list';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { forkJoin } from 'rxjs';
import { EmployeeService } from '../../core/services/employee.service';
import { DepartmentService } from '../../core/services/department.service';
import { EmployeeDto } from '../../core/models/employee.model';
import { AuthService } from '../../core/services/auth.service';

interface DashboardStats {
  totalEmployees: number;
  totalDevelopers: number;
  totalManagers: number;
  totalDepartments: number;
}

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    MatCardModule,
    MatIconModule,
    MatButtonModule,
    MatListModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss'
})
export class DashboardComponent {
  private readonly employeeService = inject(EmployeeService);
  private readonly departmentService = inject(DepartmentService);
  readonly auth = inject(AuthService);

  readonly isLoading = signal(true);
  readonly stats = signal<DashboardStats>({ totalEmployees: 0, totalDevelopers: 0, totalManagers: 0, totalDepartments: 0 });
  readonly recentHires = signal<EmployeeDto[]>([]);

  constructor() {
    this.loadDashboard();
  }

  private loadDashboard(): void {
    forkJoin({
      all: this.employeeService.search({ pageNumber: 1, pageSize: 1 }),
      developers: this.employeeService.search({ pageNumber: 1, pageSize: 1, employeeType: 'Developer' }),
      managers: this.employeeService.search({ pageNumber: 1, pageSize: 1, employeeType: 'Manager' }),
      departments: this.departmentService.getAll(),
      recent: this.employeeService.search({ pageNumber: 1, pageSize: 5, sortBy: 'HireDate', sortDescending: true })
    }).subscribe({
      next: ({ all, developers, managers, departments, recent }) => {
        this.stats.set({
          totalEmployees: all.totalCount,
          totalDevelopers: developers.totalCount,
          totalManagers: managers.totalCount,
          totalDepartments: departments.length
        });
        this.recentHires.set(recent.items);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }
}

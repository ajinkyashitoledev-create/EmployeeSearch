import { Component, OnInit, ViewChild, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { MatTableModule } from '@angular/material/table';
import { MatPaginator, MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatSort, MatSortModule, Sort } from '@angular/material/sort';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { MatChipsModule } from '@angular/material/chips';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDialog } from '@angular/material/dialog';
import { debounceTime, distinctUntilChanged } from 'rxjs';
import { EmployeeService } from '../../../core/services/employee.service';
import { DepartmentService } from '../../../core/services/department.service';
import { NotificationService } from '../../../core/services/notification.service';
import { AuthService } from '../../../core/services/auth.service';
import { DepartmentDto } from '../../../core/models/department.model';
import { EmployeeDto, EmployeeSearchRequest, EmployeeType, EmployeeTypeFilter } from '../../../core/models/employee.model';
import { EmployeeFormComponent, EmployeeFormDialogData } from '../employee-form/employee-form.component';
import { ConfirmDialogComponent } from '../../../shared/components/confirm-dialog/confirm-dialog.component';

@Component({
  selector: 'app-employee-list',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatIconModule,
    MatMenuModule,
    MatChipsModule,
    MatTooltipModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './employee-list.component.html',
  styleUrl: './employee-list.component.scss'
})
export class EmployeeListComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly employeeService = inject(EmployeeService);
  private readonly departmentService = inject(DepartmentService);
  private readonly notifications = inject(NotificationService);
  private readonly dialog = inject(MatDialog);
  private readonly route = inject(ActivatedRoute);
  readonly auth = inject(AuthService);

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  readonly displayedColumns = ['name', 'type', 'email', 'department', 'salary', 'hireDate', 'actions'];

  readonly isLoading = signal(true);
  readonly employees = signal<EmployeeDto[]>([]);
  readonly totalCount = signal(0);
  readonly departments = signal<DepartmentDto[]>([]);

  readonly filterForm = this.fb.nonNullable.group({
    searchTerm: [''],
    departmentId: [null as number | null],
    employeeType: ['All' as EmployeeTypeFilter],
    minSalary: [null as number | null],
    maxSalary: [null as number | null]
  });

  private sortBy = 'LastName';
  private sortDescending = false;
  private pageNumber = 1;
  private pageSize = 10;

  ngOnInit(): void {
    this.departmentService.getAll().subscribe(depts => this.departments.set(depts));

    const queryType = this.route.snapshot.queryParamMap.get('new') as EmployeeType | null;
    const queryTerm = this.route.snapshot.queryParamMap.get('q');
    if (queryTerm) {
      this.filterForm.patchValue({ searchTerm: queryTerm });
    }

    this.filterForm.valueChanges.pipe(debounceTime(400), distinctUntilChanged((a, b) => JSON.stringify(a) === JSON.stringify(b))).subscribe(() => {
      this.pageNumber = 1;
      if (this.paginator) this.paginator.firstPage();
      this.loadEmployees();
    });

    this.loadEmployees();

    if (queryType) {
      // Wait for departments to load before opening the create dialog.
      this.departmentService.getAll().subscribe(depts => {
        this.departments.set(depts);
        this.openCreateDialog(queryType);
      });
    }
  }

  onPageChange(event: PageEvent): void {
    this.pageNumber = event.pageIndex + 1;
    this.pageSize = event.pageSize;
    this.loadEmployees();
  }

  onSortChange(sort: Sort): void {
    if (!sort.active || sort.direction === '') {
      this.sortBy = 'LastName';
      this.sortDescending = false;
    } else {
      this.sortBy = sort.active;
      this.sortDescending = sort.direction === 'desc';
    }
    this.pageNumber = 1;
    this.loadEmployees();
  }

  loadEmployees(): void {
    this.isLoading.set(true);
    const filters = this.filterForm.getRawValue();

    const request: EmployeeSearchRequest = {
      searchTerm: filters.searchTerm || null,
      departmentId: filters.departmentId,
      employeeType: filters.employeeType,
      minSalary: filters.minSalary,
      maxSalary: filters.maxSalary,
      sortBy: this.sortBy,
      sortDescending: this.sortDescending,
      pageNumber: this.pageNumber,
      pageSize: this.pageSize
    };

    this.employeeService.search(request).subscribe({
      next: result => {
        this.employees.set(result.items);
        this.totalCount.set(result.totalCount);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }

  clearFilters(): void {
    this.filterForm.reset({ searchTerm: '', departmentId: null, employeeType: 'All', minSalary: null, maxSalary: null });
  }

  openCreateDialog(type: EmployeeType): void {
    const data: EmployeeFormDialogData = { mode: 'create', employeeType: type, departments: this.departments() };
    this.dialog
      .open(EmployeeFormComponent, { width: '640px', maxWidth: '95vw', data, autoFocus: false })
      .afterClosed()
      .subscribe(result => {
        if (result) this.loadEmployees();
      });
  }

  openEditDialog(employee: EmployeeDto): void {
    const data: EmployeeFormDialogData = {
      mode: 'edit',
      employeeType: employee.employeeType,
      employee,
      departments: this.departments()
    };
    this.dialog
      .open(EmployeeFormComponent, { width: '640px', maxWidth: '95vw', data, autoFocus: false })
      .afterClosed()
      .subscribe(result => {
        if (result) this.loadEmployees();
      });
  }

  confirmDelete(employee: EmployeeDto): void {
    this.dialog
      .open(ConfirmDialogComponent, {
        data: {
          title: 'Delete employee',
          message: `Are you sure you want to delete ${employee.fullName}? This cannot be undone.`,
          confirmLabel: 'Delete',
          destructive: true
        }
      })
      .afterClosed()
      .subscribe(confirmed => {
        if (!confirmed) return;
        this.employeeService.delete(employee.id).subscribe({
          next: () => {
            this.notifications.success(`${employee.fullName} was deleted.`);
            this.loadEmployees();
          }
        });
      });
  }
}

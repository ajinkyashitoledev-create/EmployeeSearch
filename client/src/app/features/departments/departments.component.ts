import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { DepartmentService } from '../../core/services/department.service';
import { NotificationService } from '../../core/services/notification.service';
import { DepartmentDto } from '../../core/models/department.model';

@Component({
  selector: 'app-departments',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './departments.component.html',
  styleUrl: './departments.component.scss'
})
export class DepartmentsComponent {
  private readonly fb = inject(FormBuilder);
  private readonly departmentService = inject(DepartmentService);
  private readonly notifications = inject(NotificationService);

  readonly isLoading = signal(true);
  readonly isSubmitting = signal(false);
  readonly showForm = signal(false);
  readonly departments = signal<DepartmentDto[]>([]);
  readonly displayedColumns = ['name', 'location', 'employeeCount'];

  readonly form = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(100)]],
    location: ['', [Validators.required, Validators.maxLength(100)]]
  });

  constructor() {
    this.load();
  }

  load(): void {
    this.isLoading.set(true);
    this.departmentService.getAll().subscribe({
      next: depts => {
        this.departments.set(depts);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }

  toggleForm(): void {
    this.showForm.update(v => !v);
    if (!this.showForm()) this.form.reset();
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSubmitting.set(true);
    this.departmentService.create(this.form.getRawValue()).subscribe({
      next: dept => {
        this.notifications.success(`Department "${dept.name}" was created.`);
        this.form.reset();
        this.showForm.set(false);
        this.isSubmitting.set(false);
        this.load();
      },
      error: () => this.isSubmitting.set(false)
    });
  }
}

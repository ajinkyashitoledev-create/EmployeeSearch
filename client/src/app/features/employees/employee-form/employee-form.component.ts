import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { Observable, switchMap } from 'rxjs';
import { EmployeeService } from '../../../core/services/employee.service';
import { NotificationService } from '../../../core/services/notification.service';
import { DepartmentDto } from '../../../core/models/department.model';
import { DeveloperDto, EmployeeDto, EmployeeType, ManagerDto } from '../../../core/models/employee.model';

export interface EmployeeFormDialogData {
  mode: 'create' | 'edit';
  employeeType: EmployeeType;
  employee?: EmployeeDto;
  departments: DepartmentDto[];
}

@Component({
  selector: 'app-employee-form',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatIconModule,
    MatButtonToggleModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './employee-form.component.html',
  styleUrl: './employee-form.component.scss'
})
export class EmployeeFormComponent {
  readonly data = inject<EmployeeFormDialogData>(MAT_DIALOG_DATA);
  private readonly dialogRef = inject(MatDialogRef<EmployeeFormComponent>);
  private readonly fb = inject(FormBuilder);
  private readonly employeeService = inject(EmployeeService);
  private readonly notifications = inject(NotificationService);

  readonly isSubmitting = signal(false);
  readonly employeeType = signal<EmployeeType>(this.data.employeeType);
  readonly isEdit = this.data.mode === 'edit';

  readonly form = this.fb.nonNullable.group({
    firstName: [this.data.employee?.firstName ?? '', [Validators.required, Validators.maxLength(100)]],
    lastName: [this.data.employee?.lastName ?? '', [Validators.required, Validators.maxLength(100)]],
    email: [this.data.employee?.email ?? '', [Validators.required, Validators.email, Validators.maxLength(200)]],
    phoneNumber: [this.data.employee?.phoneNumber ?? '', [Validators.required, Validators.maxLength(20)]],
    hireDate: [this.data.employee ? new Date(this.data.employee.hireDate) : new Date(), [Validators.required]],
    salary: [this.data.employee?.salary ?? 0, [Validators.required, Validators.min(0.01)]],
    departmentId: [this.data.employee?.departmentId ?? this.data.departments[0]?.id ?? null, [Validators.required]],

    programmingLanguage: [
      (this.data.employee as DeveloperDto)?.programmingLanguage ?? '',
      [Validators.required, Validators.maxLength(50)]
    ],
    yearsOfExperience: [(this.data.employee as DeveloperDto)?.yearsOfExperience ?? 0, [Validators.required, Validators.min(0), Validators.max(60)]],
    gitHubProfile: [(this.data.employee as DeveloperDto)?.gitHubProfile ?? ''],

    teamSize: [(this.data.employee as ManagerDto)?.teamSize ?? 0, [Validators.required, Validators.min(0), Validators.max(500)]],
    bonus: [(this.data.employee as ManagerDto)?.bonus ?? 0, [Validators.required, Validators.min(0)]]
  });

  setType(type: EmployeeType): void {
    if (this.isEdit) return;
    this.employeeType.set(type);
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSubmitting.set(true);
    const raw = this.form.getRawValue();
    const common = {
      firstName: raw.firstName,
      lastName: raw.lastName,
      email: raw.email,
      phoneNumber: raw.phoneNumber,
      hireDate: (raw.hireDate as Date).toISOString(),
      salary: raw.salary,
      departmentId: raw.departmentId as number
    };

    if (this.data.mode === 'create') {
      const create$: Observable<EmployeeDto> =
        this.employeeType() === 'developer'
          ? this.employeeService.createDeveloper({
              ...common,
              programmingLanguage: raw.programmingLanguage,
              yearsOfExperience: raw.yearsOfExperience,
              gitHubProfile: raw.gitHubProfile || null
            })
          : this.employeeService.createManager({ ...common, teamSize: raw.teamSize, bonus: raw.bonus });

      create$.subscribe({
        next: result => {
          this.notifications.success(`${result.fullName} was added.`);
          this.dialogRef.close(result);
        },
        error: () => this.isSubmitting.set(false)
      });
      return;
    }

    const id = this.data.employee!.id;
    const details$: Observable<EmployeeDto> =
      this.employeeType() === 'developer'
        ? this.employeeService.updateDeveloperDetails(id, {
            programmingLanguage: raw.programmingLanguage,
            yearsOfExperience: raw.yearsOfExperience,
            gitHubProfile: raw.gitHubProfile || null
          })
        : this.employeeService.updateManagerDetails(id, { teamSize: raw.teamSize, bonus: raw.bonus });

    this.employeeService
      .update(id, common)
      .pipe(switchMap(() => details$))
      .subscribe({
        next: result => {
          this.notifications.success(`${result.fullName} was updated.`);
          this.dialogRef.close(result);
        },
        error: () => this.isSubmitting.set(false)
      });
  }

  cancel(): void {
    this.dialogRef.close();
  }
}

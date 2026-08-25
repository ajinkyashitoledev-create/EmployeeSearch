export type EmployeeType = 'developer' | 'manager';

export interface EmployeeDto {
  employeeType: EmployeeType;
  id: number;
  firstName: string;
  lastName: string;
  fullName: string;
  email: string;
  phoneNumber: string;
  hireDate: string;
  salary: number;
  departmentId: number;
  departmentName: string | null;
}

export interface DeveloperDto extends EmployeeDto {
  employeeType: 'developer';
  programmingLanguage: string;
  yearsOfExperience: number;
  gitHubProfile: string | null;
}

export interface ManagerDto extends EmployeeDto {
  employeeType: 'manager';
  teamSize: number;
  bonus: number;
}

export type EmployeeTypeFilter = 'All' | 'Developer' | 'Manager';

export interface EmployeeSearchRequest {
  searchTerm?: string | null;
  departmentId?: number | null;
  employeeType?: EmployeeTypeFilter;
  minSalary?: number | null;
  maxSalary?: number | null;
  hiredAfter?: string | null;
  hiredBefore?: string | null;
  sortBy?: string;
  sortDescending?: boolean;
  pageNumber?: number;
  pageSize?: number;
}

interface EmployeeCoreFields {
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber: string;
  hireDate: string;
  salary: number;
  departmentId: number;
}

export interface CreateDeveloperRequest extends EmployeeCoreFields {
  programmingLanguage: string;
  yearsOfExperience: number;
  gitHubProfile?: string | null;
}

export interface CreateManagerRequest extends EmployeeCoreFields {
  teamSize: number;
  bonus: number;
}

export interface UpdateEmployeeRequest extends EmployeeCoreFields {}

export interface UpdateDeveloperDetailsRequest {
  programmingLanguage: string;
  yearsOfExperience: number;
  gitHubProfile?: string | null;
}

export interface UpdateManagerDetailsRequest {
  teamSize: number;
  bonus: number;
}

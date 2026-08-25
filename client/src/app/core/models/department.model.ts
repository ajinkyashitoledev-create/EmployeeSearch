export interface DepartmentDto {
  id: number;
  name: string;
  location: string;
  employeeCount: number;
}

export interface CreateDepartmentRequest {
  name: string;
  location: string;
}

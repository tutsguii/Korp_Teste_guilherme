export interface ApiSuccessResponse<T> {
  success: boolean;
  message: string;
  data: T;
}

export interface ApiErrorResponse {
  success: boolean;
  message: string;
  errors?: string[];
  traceId?: string;
}

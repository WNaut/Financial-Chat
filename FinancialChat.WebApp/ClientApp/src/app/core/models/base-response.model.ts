export default interface BaseResponseModel<T> {
  message: string;
  data: T;
}

export default interface ChangePasswordModel {
  userId?: string;
  oldPassword: string;
  newPassword: string;
}

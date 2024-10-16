namespace NoteTaking.Api.Context;

public record class UserDTO
(
    string UserName,
    string UserID,
    DateTime CreatedDate,
    string UserEmail);
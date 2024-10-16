namespace NoteTaking.Api.Context;

public record class NoteDTO
(
    int Id,
    string Note,
    DateTime CreatedDate,
    DateTime UpdatedDate
);


using NoteTaking.Api.Validations;

namespace NoteTaking.Api.Context;

public record class UpdateNoteDTO
(
    string UserNote,
    string UserID
);

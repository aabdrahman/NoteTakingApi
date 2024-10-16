using System.ComponentModel.DataAnnotations;
using NoteTaking.Api.Validations;

namespace NoteTaking.Api.Context;

public record class CreateNoteDTO
(
    [Required]
    string UserId,
    [Required]
    string Note
);

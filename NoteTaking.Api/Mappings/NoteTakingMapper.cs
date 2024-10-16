using System;
using Microsoft.AspNetCore.Identity;
using NoteTaking.Api.Context;
using NoteTaking.Api.Interfaces;
using NoteTaking.Api.Model;

namespace NoteTaking.Api.Mappings;

public static class NoteTakingMapper
{

    public static UserDTO ToUserDTO(this User user)
    {
        return new UserDTO
        (
            UserName: user.UserName,
            UserID: user.UserIdentificationNumber,
            CreatedDate: user.CreatedDate,
            UserEmail: user.UserEmail ?? "noemail@connect.com"
        );
    }

    public static NoteDTO ToNoteDTO(this Note note)
    {
        return new NoteDTO
        (
            Id: note.Id,
            Note: note.UserNote!,
            CreatedDate: note.CreatedDate,
            UpdatedDate: note.UpdateDate
        );
    }

    public static List<NoteDTO> ToNoteList(this ICollection<Note> notes)
    {
        List<NoteDTO> NoteList = new List<NoteDTO>();

        foreach(Note note in notes)
            NoteList.Add
            (
                new NoteDTO(Id: note.Id,Note: note.UserNote!, CreatedDate: note.CreatedDate,UpdatedDate:note.UpdateDate)
            );
        
        return NoteList;
    }

    public static Note ToNoteEntity(this CreateNoteDTO createNoteDTO)
    {
        return new Note()
        {
            UserNote = createNoteDTO.Note,
            UserIdentificationNumber = createNoteDTO.UserId
        };  
    }

    public static Note ToNoteEntity(this NoteDTO noteDTO, string UserID)
    {
        return new Note()
        {
            Id = noteDTO.Id,
            UserNote = noteDTO.Note,
            CreatedDate = noteDTO.CreatedDate,
            UserIdentificationNumber = UserID
        };
    }

    public static Note ToNoteEntity(this UpdateNoteDTO updateNoteDTO, int NoteID)
    {
        return new Note()
        {
            Id = NoteID,
            UserNote = updateNoteDTO.UserNote,
            UserIdentificationNumber = updateNoteDTO.UserID
        };  
    }

    public static User ToUserEntity(this CreateUserDTO createUserDTO, string UserID, IPasswordHasher passwordHasher)
    {
        return new User()
        {
            UserName = createUserDTO.UserName,
            UserPassword = passwordHasher.Hash(createUserDTO.Password),
            UserIdentificationNumber = UserID,
            UserEmail = createUserDTO.UserEmail,
            EmailVerified = false
        };
    }
}

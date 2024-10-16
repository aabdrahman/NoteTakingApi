using System;
using NoteTaking.Api.Context;
using NoteTaking.Api.Model;

namespace NoteTaking.Api.Interfaces;

public interface INoteTakingServices
{
    Task<List<NoteDTO>> GetUserNotes(string UserId);

    Task<NoteDTO> GetNote(string UserID, int NoteID);
    Task<UserDTO> GetUser(string UserID);
    Task<(User, string)> CreateUser(CreateUserDTO NewUser);

    Task CreateNote(CreateNoteDTO NewNote);
    Task UpdateNote(int NoteID, UpdateNoteDTO updateNoteDTO);
    Task<Note> GetNoteById(int Id);

    Task DeleteNote(int NoteID, string UserID);
    Task DeleteUser(string UserID);

    Task<List<UserDTO>> GetAllUsers();

    Task<List<User>> FetchUsers();
    Task<List<Note>> FetchNotes();
    Task<UserDTO> LoginUser(CreateUserDTO LoginUser);
    Task<string> AuthenticateUser();

    Task<string> ActivateUser(User user);

    Task RemoveDeletedUser(string UserID);

    Task RemoveDeletedNote(int ID, string UserID);

    Task<List<UserSummary>> GetUserSummary();

    List<UserDTO> GetUsersToDelete();
    List<NoteDTO> GetNotesToDelete();

    Task RemoveNoteViaBackgroundService(int NoteID);

}

using System;
using System.Text;
using System.Text.Json.Serialization;
using FluentEmail.Core;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Newtonsoft.Json;
using NoteTaking.Api.Context;
using NoteTaking.Api.Interfaces;
using NoteTaking.Api.Mappings;
using NoteTaking.Api.Middleware;
using NoteTaking.Api.Model;

namespace NoteTaking.Api.Services;

public class NoteTakingServices : INoteTakingServices
{
    
    private readonly ILogger<NoteTakingServices> _logger;
    private readonly NoteTakingDbContext _context;

    private readonly IPasswordHasher _passwordHasher;
    private readonly TokenProvider _tokenProvider;

    private readonly IFluentEmail _fluentemail;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly EmailVerificationLinkFactory _emailVerificationLinkFactory;

    public NoteTakingServices(ILogger<NoteTakingServices> logger, 
                            NoteTakingDbContext context, 
                            IPasswordHasher passwordHasher,
                            TokenProvider tokenProvider,
                            IFluentEmail fluentEmail, 
                            IHttpClientFactory httpClientFactory,
                            EmailVerificationLinkFactory emailVerificationLinkFactory)
    {
        _logger = logger;
        _context = context;
        _passwordHasher = passwordHasher;
        _tokenProvider = tokenProvider;
        _fluentemail = fluentEmail;
        _httpClientFactory = httpClientFactory;
        _emailVerificationLinkFactory = emailVerificationLinkFactory;
    }

    private async Task<string> UniqueUserIdGenerator()
    {
        Random randNum = new Random();
        //string userid = randNum.Next(100000, 999999).ToString();

        var userid = await Task.Run(() => randNum.Next(100000, 999999));

        return userid.ToString();
    }

    

    public async Task<NoteDTO> GetNote(string UserID, int NoteID)
    {
        try
        {
            var Note = await _context.Notes.SingleAsync(n => n.UserIdentificationNumber == UserID && n.Id == NoteID);
            if(Note is null)
            {
                throw new ArgumentNullException(nameof(NoteID), $"Note with Id: {NoteID} cannot be found.");
            }
            return Note.ToNoteDTO();
            
        }

        catch(ArgumentNullException ex)
        {
            _logger.LogError(ex, "No Note found.");
            throw;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "An error occurred fetching user note.");
            throw new Exception("An error occurred fetching user note.");
        }
       
    }
    
    public async Task CreateNote(CreateNoteDTO NewNote)
    {
        try
        {
            var CreatedNote = NewNote.ToNoteEntity();
            CreatedNote.CreatedDate = DateTime.Now;

            _context.Notes.Add(CreatedNote);
            await _context.SaveChangesAsync();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "An error occurred processing Note Create Operation");
            throw new Exception("An error occurred processing Note Create Operation");
        }

    }

    public async Task<(User, string)> CreateUser(CreateUserDTO NewUser)
    {
        try
        {
            bool EsitsEmail = await _context.Users.AnyAsync(u => u.UserEmail == NewUser.UserEmail);
            if(EsitsEmail)
            {
                throw new InvalidOperationException("A user with the email already exists.");
            }

            string userID = await UniqueUserIdGenerator();
            var CreatedUser = NewUser.ToUserEntity(userID, _passwordHasher);
            //string password = _passwordHasher.Hash(CreatedUser.UserPassword);
            CreatedUser.CreatedDate = DateTime.Now;

            _context.Users.Add(CreatedUser);

            var userToken = await ActivateUser(CreatedUser);


            //Sending verification email
           // await _fluentemail
             //           .To(NewUser.UserEmail)
               //         .Subject("User Registration Verification Email for Varst Notetaking Application.")
                 //       .Body($"Your UserID is: {userID}\nTo proceed, Click here to verify your account.")
                   //     .SendAsync();

            await _context.SaveChangesAsync();

            return (CreatedUser, userToken);

        }
        catch(InvalidOperationException ex)
        {
            _logger.LogError(ex, "A user with email already exists.");
            throw;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "An error occurred processing User Create Operation");
            throw new Exception("An error occurred processing User Create Operation");
        }
    }

    public async Task DeleteNote(int NoteID, string UserID)
    {
        try
        {
            var note = await _context.Notes.FindAsync(NoteID);

            if(note is null)
            {
                throw new ArgumentNullException(nameof(NoteID), $"No note with specified Id: {NoteID}");
            }

            await _context.Notes
                .Where(n => n.Id == NoteID && n.UserIdentificationNumber == UserID)
                .ExecuteUpdateAsync
                (
                    n => n.SetProperty(n => n.IsDeleted, true)
                        .SetProperty(n => n.DeleteDate, DateTime.Now)
                );
        }
        catch(ArgumentNullException ex)
        {
            _logger.LogError(ex, $"Note deletion failed: Note not found.");
            throw;
        }

        catch(Exception ex)
        {
            _logger.LogError(ex, "An error occurred deleting user");
            throw new Exception("An error occurred deleting user");
        }
        
    }

    public async Task DeleteUser(string UserID)
    {
        try
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.UserIdentificationNumber == UserID);

            if(user is null)
            {
                throw new ArgumentNullException(nameof(UserID), $"No user with specified Id: {UserID}");
            }

            await _context.Users
                        .Where(u => u.UserIdentificationNumber == UserID)
                        .ExecuteUpdateAsync
                        (
                            u => u.SetProperty(u => u.IsDeleted, true)
                                .SetProperty(u => u.DeleteDate, DateTime.Now)
                        );

        }

        catch(ArgumentNullException ex)
        {
            _logger.LogError(ex, $"User deletion failed: User not found.");
            throw;
        }

        catch(Exception ex)
        {
            _logger.LogError(ex, "An error occurred deleting user");
            throw new Exception("An error occurred deleting user");
        }
    }

    public async Task<UserDTO> GetUser(string UserID)
    {
        try
        {
            var user = await _context.Users
                                .SingleOrDefaultAsync(u => u.UserIdentificationNumber == UserID);

            if(user is null)
            {
                throw new ArgumentNullException(nameof(UserID), $"User with Id: {UserID} cannot be found.");
            }
            
            return user.ToUserDTO();

        }
        catch(ArgumentNullException ex)
        {
            _logger.LogError(ex, "No User was found.");
            throw;
        }

        catch(Exception ex)
        {
            _logger.LogError(ex, "Error Fetching User details.");
            throw new Exception("Error Fetching User details.");
        }
    }

    public async Task<List<NoteDTO>> GetUserNotes(string UserId)
    {
        var UserNotes = await _context.Notes
                                .Where(n => n.UserIdentificationNumber == UserId)
                                .Select(n => n.ToNoteDTO())
                                .ToListAsync();
        return UserNotes;
    }

    public async Task UpdateNote(int NoteID, UpdateNoteDTO updateNoteDTO)
    {
        try
        {
            var ExistingNote = _context.Notes.Find(NoteID);

            if(ExistingNote is null)
            {
                throw new ArgumentNullException(nameof(NoteID), $"Note with Id: {NoteID} could not be found");
            }

            var UpdatedNote = updateNoteDTO.ToNoteEntity(NoteID);
            UpdatedNote.UpdateDate = DateTime.Now;
            UpdatedNote.CreatedDate = ExistingNote.CreatedDate;

           _context.Entry(ExistingNote).State = EntityState.Detached;
            _context.Notes.Update(UpdatedNote);

            await _context.SaveChangesAsync();
        }

        catch(ArgumentNullException ex)
        {
            _logger.LogError(ex, "Note cannot be found");
            throw;
        }

        catch(Exception ex)
        {
            _logger.LogError(ex, "An Error occurred fetching Note.");
            throw new Exception("An Error occurred fetching Note.");
        }
        

    }

    public async Task<List<UserDTO>> GetAllUsers()
    {
        try
        {
            var AllUsers = await _context.Users
                                    .Select(u => u.ToUserDTO())
                                    .ToListAsync();

            return AllUsers;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "An error occurred fetching all users");
            throw new Exception("An error occurred fetching all users");
        }
    }

    public async Task<Note> GetNoteById(int Id)
    {
        try
        {
            var note = await _context.Notes.FindAsync(Id);
            if (note is null)
            {
                throw new ArgumentNullException($"Note with Id: {Id} could not be found");
            }
            return note;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "An Error occurred fetching Note.");
            throw new Exception("An Error occurred fetching Note.");
        }
        
    }

    public async Task<List<User>> FetchUsers()
    {
        try
        {
            var Users = await _context.Users
                                       .IgnoreQueryFilters()
                                       .Include(u => u.UserNotes)
                                       .AsNoTrackingWithIdentityResolution()
                                       .AsSplitQuery()
                                       //.Where(u => u.UserIdentificationNumber == "429868")
                                       .ToListAsync();
            return Users;
        }

        catch(Exception ex)
        {
            _logger.LogError(ex, "An Error occurred fetching Users");
            throw new Exception("An Error occurred fetching Users.");
        }

    }

    public async Task<List<Note>> FetchNotes()
    {
        try
        {
            var Notes = await _context.Notes
                                       .IgnoreQueryFilters()
                                       .AsNoTracking()
                                       //.Select(u => u.ToNoteDTO())
                                       .ToListAsync();
            return Notes;
        }

        catch(Exception ex)
        {
            _logger.LogError(ex, "An Error occurred fetching Notes");
            throw new Exception("An Error occurred fetching Notes.");
        }
    }

    public async Task<string> ActivateUser(User user)
    {
        DateTime CurrentDateTime = DateTime.UtcNow;
        var UserToken = new UserVerification
        {
            UserID = user.UserIdentificationNumber,
            CreatedOn = CurrentDateTime,
            ExpiresOn = CurrentDateTime.AddDays(1),
            Id = Guid.NewGuid(),
            Token = Encoding.UTF8.GetBytes(user.UserEmail!).ToString()!
        };

        string verificationLink = _emailVerificationLinkFactory.CreteToken(UserToken);

        _context.UserVerificationTokens.Add(UserToken);
        await _context.SaveChangesAsync();

        return verificationLink;

        
    }
    
    public async Task<UserDTO> LoginUser(CreateUserDTO LoginUser)
    {
        
        try
        {
            User? user = await _context.Users
                                        .IgnoreQueryFilters()
                                        .AsNoTracking()
                                        .SingleOrDefaultAsync(u => u.UserName == LoginUser.UserName);

            if(user is null)
            {
                throw new ArgumentNullException(nameof(user), "Username is incorrect");
            }
            
            bool IsVerified = _passwordHasher.Verify(LoginUser.Password, user.UserPassword);

            if(!IsVerified)
            {
                throw new ArgumentException(nameof(LoginUser.Password), "The provided password is incorrect.");
            }

            if(user.IsDeleted || !user.EmailVerified)
            {
                var userToken = await ActivateUser(user);
                throw new NotImplementedException(userToken);
            }

            return user.ToUserDTO();

        }
        catch (NotImplementedException ex)
        {
            _logger.LogError(ex, ex.Message);
            throw;
        }
        catch(ArgumentException ex)
        {
            _logger.LogError(ex, ex.Message);
            throw;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "An Error Occurred Logging in user.");
            throw new Exception("An Error Occurred Logging in user.");
        }     
    }
    
    public async Task<string> AuthenticateUser()
    {
        try
        {
            TokenUser user = new TokenUser(){UserName = "", Password = ""};

            
            var token =  await _tokenProvider.GenerateToken();
            return token;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "An error occurred authenticating user.");
            throw new Exception("An error occurred authenticating user.");
        }
    }

    public async Task RemoveDeletedUser(string UserID)
    {
        try
        {
            var user = await _context.Users
                                 .IgnoreQueryFilters()
                                 .SingleOrDefaultAsync(u => u.UserIdentificationNumber == UserID);
            if(user is null)
            {
                throw new ArgumentNullException(nameof(UserID),"User not found");
            }
            else if(!user.IsDeleted)
            {
                throw new ArgumentException(nameof(user.IsDeleted), "The provided User is not yet deleted");
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

        }
        catch(ArgumentNullException ex)
        {
            _logger.LogError(ex, ex.Message);
            throw;
        }
        catch(ArgumentException ex)
        {
            _logger.LogError(ex, ex.Message);
            throw;
        }
        catch(DbUpdateException ex)
       {
            _logger.LogError(ex, ex.Message);
            throw;
       }
        catch(Exception ex)
        {
            _logger.LogError(ex, "An Error Occurred Deleting User.");
            throw new Exception("An Error Occurred Deleting User.");
        }
    }

    public async Task RemoveDeletedNote(int ID, string UserID)
    {
       try
       {
            var note = await _context.Notes
                                     .IgnoreQueryFilters()
                                     .SingleOrDefaultAsync(n => n.UserIdentificationNumber == UserID && n.Id == ID);
            if(note is null)
            {
                throw new ArgumentNullException(nameof(ID), "No note with specified Id.");
            }
            else if(!note.IsDeleted)
            {
                throw new ArgumentException(nameof(note), "The specified Note is not deleted yet.");
            }

            _context.Notes.Remove(note);
            await _context.SaveChangesAsync();
       }
       catch(ArgumentNullException ex)
       {
            _logger.LogError(ex, ex.Message);
            throw;
       }
       catch(ArgumentException ex)
       {
            _logger.LogError(ex, ex.Message);
            throw;
       }
       catch(DbUpdateException ex)
       {
            _logger.LogError(ex, ex.Message);
            throw;
       }
       catch(Exception ex)
       {
            _logger.LogError(ex, "An Error Occurred Deleting Note.");
            throw new Exception("An Error Occurred Deleting Note.");
       }
    }

    public async Task<List<UserSummary>> GetUserSummary()
    {
        try
        {
            var UserSummaries = await _context.UserSummary
                                            .Select(n => new UserSummary{UserID = n.UserID, Number_Of_Notes = n.Number_Of_Notes})
                                            .ToListAsync();

            return UserSummaries;
        }

        catch(Exception ex)
        {
            _logger.LogError(ex, $"An error occurrred reading from database view: {ex.Message}");
            throw new Exception($"An error occurrred reading from database view: {ex.Message}");
        }
    }

    public List<UserDTO> GetUsersToDelete()
    {
        var UsersToDelete = _context.Users
                                    .IgnoreQueryFilters()
                                    .Where(u => u.DeleteDate.AddDays(2).Date == DateTime.Now.Date)
                                    .Select( u => u.ToUserDTO()).ToList();
        return UsersToDelete;
    }

    public List<NoteDTO> GetNotesToDelete()
    {
        var NotesToDelete = _context.Notes
                                    .IgnoreQueryFilters()
                                    .Where(n => n.DeleteDate.AddDays(2).Date == DateTime.Now.Date)
                                    .Select(n => n.ToNoteDTO())
                                    .ToList();
        return NotesToDelete;
    }

    public async Task RemoveNoteViaBackgroundService(int NoteID)
    {
        var note = await _context.Notes.FindAsync(NoteID);

        _context.Notes.Remove(note!);
        await _context.SaveChangesAsync();

    }

}

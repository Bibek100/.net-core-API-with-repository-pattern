using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using dotnet_rpg.Data;
using dotnet_rpg.Dtos.Character;
using dotnet_rpg.Models;
using dotnet_rpg.Service.CharacterService;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace dotnet_rpg.Service.CharacterService
{
    public class CharacterService : ICharacterService
    {
        private readonly IMapper _mapper;
        private readonly DataContext _context;

        private readonly IHttpContextAccessor _httpContextAccessor;

        public CharacterService(IMapper mapper, DataContext context, IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            _context = context;
            _mapper = mapper;
        }
        private static List<Character> characters = new List<Character> {
                new Character(),
                new Character { Id = 1, Name = "Sam"}
            };
        private int GetUserId() => int.Parse(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
        public async Task<ServiceResponse<List<GetCharacterDto>>> AddCharacter(AddCharacterDto newCharacter)
        {
            ServiceResponse<List<GetCharacterDto>> serviceResponse = new ServiceResponse<List<GetCharacterDto>>();

            Character character = _mapper.Map<Character>(newCharacter);
            character.User = await _context.Users.FirstOrDefaultAsync(u => u.Id == GetUserId());
            // character.Id = characters.Max(c => c.Id) + 1;
            // characters.Add(character);
            // serviceResponse.Data = (characters.Select(c => _mapper.Map<GetCharacterDto>(c))).ToList();
            await _context.Characters.AddAsync(character);
            await _context.SaveChangesAsync();

            //serviceResponse.Data = (_context.Characters.Select(c => _mapper.Map<GetCharacterDto>(c))).ToList();
            serviceResponse.Data = (_context.Characters.Where(c => c.User.Id == GetUserId()).Select(c => _mapper.Map<GetCharacterDto>(c))).ToList();
            return serviceResponse;
        }

        public async Task<ServiceResponse<List<GetCharacterDto>>> GetAllCharacters()
        {
            ServiceResponse<List<GetCharacterDto>> serviceResponse = new ServiceResponse<List<GetCharacterDto>>();
            // serviceResponse.Data = (characters.Select(c => _mapper.Map<GetCharacterDto>(c))).ToList();

            //List<Character> dbCharacters = await _context.Characters.ToListAsync();
            //List<Character> dbCharacters = await _context.Characters.Where(c => c.User.Id == userId).ToListAsync();
            List<Character> dbCharacters = await _context.Characters.Include(w => w.Weapon)
             .Include(c => c.CharacterSkills)
             .ThenInclude(cs => cs.Skill)
            .Where(c => c.User.Id == GetUserId()).ToListAsync();
            serviceResponse.Data = dbCharacters.Select(c => _mapper.Map<GetCharacterDto>(c)).ToList();

            return serviceResponse;
        }

        public async Task<ServiceResponse<GetCharacterDto>> GetCharacterById(int id)
        {
            ServiceResponse<GetCharacterDto> serviceResponse = new ServiceResponse<GetCharacterDto>();

            //serviceResponse.Data = _mapper.Map<GetCharacterDto>(characters.FirstOrDefault(c => c.Id == id));

            //Character dbCharacter = await _context.Characters.FirstOrDefaultAsync(c => c.Id == id);
            Character dbCharacter =
            await _context.Characters
                  .Include(c => c.Weapon)
                  .Include(c => c.CharacterSkills).ThenInclude(cs => cs.Skill).
                   FirstOrDefaultAsync(c => c.Id == id && c.User.Id == GetUserId());

            serviceResponse.Data = _mapper.Map<GetCharacterDto>(dbCharacter);
            return serviceResponse;
        }

        public async Task<ServiceResponse<GetCharacterDto>> UpdateCharacter(UpdateCharacterDto updatedCharacter)
        {
            ServiceResponse<GetCharacterDto> serviceResponse = new ServiceResponse<GetCharacterDto>();
            try
            {
                //Character character = characters.FirstOrDefault(c => c.Id == updatedCharacter.Id);
                Character character = await _context.Characters.Include(c => c.User).FirstOrDefaultAsync(c => c.Id == updatedCharacter.Id);
                if (character.User.Id == GetUserId())
                {

                    character.Name = updatedCharacter.Name;
                    character.Class = updatedCharacter.Class;
                    character.Defense = updatedCharacter.Defense;
                    character.HitPoints = updatedCharacter.HitPoints;
                    character.Intelligence = updatedCharacter.Intelligence;
                    character.Strength = updatedCharacter.Strength;
                    _context.Characters.Update(character);
                    await _context.SaveChangesAsync();
                    serviceResponse.Data = _mapper.Map<GetCharacterDto>(character);
                }
                else
                {
                    serviceResponse.Success = false;
                    serviceResponse.Message = "Character not found.";
                }
            }
            catch (Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = ex.Message;
            }
            return serviceResponse;



        }

        public async Task<ServiceResponse<List<GetCharacterDto>>> DeleteCharacter(int id)
        {
            ServiceResponse<List<GetCharacterDto>> serviceResponse = new ServiceResponse<List<GetCharacterDto>>();
            try
            {
                // Character character = characters.First(c => c.Id == id);

                //characters.Remove(character);

                //Character character = await _context.Characters.FirstAsync(c => c.Id == id);
                Character character =
            await _context.Characters.FirstOrDefaultAsync(c => c.Id == id && c.User.Id == GetUserId());
                if (character != null)
                {
                    _context.Characters.Remove(character);
                    await _context.SaveChangesAsync();


                    // serviceResponse.Data = (characters.Select(c => _mapper.Map<GetCharacterDto>(c))).ToList();
                    //serviceResponse.Data = (_context.Characters.Select(c => _mapper.Map<GetCharacterDto>(c))).ToList();
                    serviceResponse.Data = (_context.Characters.Where(c => c.User.Id == GetUserId())
                   .Select(c => _mapper.Map<GetCharacterDto>(c))).ToList();
                }
                else
                {
                    serviceResponse.Success = false;
                    serviceResponse.Message = "Character not found.";
                }

            }
            catch (Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = ex.Message;
            }
            return serviceResponse;
        }
    }
}
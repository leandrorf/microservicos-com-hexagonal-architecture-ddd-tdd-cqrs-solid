﻿using Application.Responses;
using Application.Room.Dtos;
using Application.Room.Ports;
using Application.Room.Request;
using Application.Room.Responses;
using Domain.Room.Exceptions;
using Domain.Room.Ports;
using Domain.Users;

namespace Application.Room
{
    public class RoomManager : IRoomManager
    {
        private readonly IRoomRepository _roomRepository;
        private readonly IUserRepository _userRepository;
        public RoomManager(
            IRoomRepository roomRepository,
            IUserRepository userRepository)
        { 
            _roomRepository = roomRepository;
            _userRepository = userRepository;
        }

        public async Task<RoomResponse> CreateRoom(CreateRoomRequest request)
        {
            try
            {
                var user = await _userRepository.Get(request.UserName);

                if (!user.Roles.Contains("manager"))
                {
                    return new RoomResponse
                    {
                        Success = false,
                        ErrorCode = ErrorCodes.ROOM_INVALID_PERMISSION,
                        Message = "User does not have permission to perform this action"
                    };
                }

                var room = RoomDto.MapToEntity(request.Data);

                await room.Save(_roomRepository);
                request.Data.Id = room.Id;

                return new RoomResponse
                {
                    Success = true,
                    Data = request.Data,
                };
            }
            catch (InvalidRoomDataException)
            {

                return new RoomResponse
                {
                    Success = false,
                    ErrorCode = ErrorCodes.ROOM_MISSING_REQUIRED_INFORMATION,
                    Message = "Missing required information passed"
                };
            }
            catch (Exception)
            {
                return new RoomResponse
                {
                    Success = false,
                    ErrorCode = ErrorCodes.ROOM_COULD_NOT_STORE_DATA,
                    Message = "There was an error when saving to DB"
                };
            }
        }

        public async Task<RoomResponse> GetRoom(int roomId)
        {
            var room = await _roomRepository.Get(roomId);

            if (room == null) 
            {
                return new RoomResponse
                {
                    Success = false,
                    ErrorCode = ErrorCodes.ROOM_NOT_FOUND,
                    Message = "Room not found"
                };
            }

            return new RoomResponse
            {
                Success = true,
                Data = RoomDto.MapToDto(room),
            };
        }
    }
}

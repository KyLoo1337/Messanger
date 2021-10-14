using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Messanger.Models;

namespace Messanger.Controllers
{
    public class UserControll : Controller
    {
        /// <summary>
        /// Cписок пользователей.
        /// </summary>
        private static List<User> _users = new List<User>();
        
        /// <summary>
        /// Список сообщений.
        /// </summary>
        private static List<Message> _messages = new List<Message>();
        
        /// <summary>
        /// Номер id следующего добавленного пользователя.
        /// </summary>
        private static int idNum = 1;

        /// <summary>
        /// Генерация строки рандомных букв.
        /// </summary>
        /// <param name="len"> Длинна строки. </param>
        /// <returns> Сгенерированную строку. </returns>
        private string GenerateRandomString(int len)
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var stringChars = new char[len];
            var random = new Random();

            for (int j = 0; j < stringChars.Length; j++)
            {
                stringChars[j] = chars[random.Next(chars.Length)];
            }

            var finalString = new String(stringChars);
            return finalString;
        }
        
        /// <summary>
        /// Создать пользователя.
        /// </summary>
        /// <param name="req"> Входные данные (имя пользователя и email). </param>
        /// <returns> Ок и пользователя в случае, если всё хорошо, иначе NotFound. </returns>
        [HttpPost("create-user")]
        public IActionResult CreateUser([FromBody] CreateUserRequest req)
        {
            var sameEmail = _users.Find(x => x.Email == req.Email);
            if(sameEmail != null)
                return NotFound(new { Message = $"Пользователь с таким Email уже зарегистрирован!" });
            var user = new User()
            {
                Id = idNum++,
                Email = req.Email,
                UserName = req.UserName,
                Messages = new Dictionary<User, List<Message>>()
        };
            _users.Add(user);
            _users = _users.OrderBy(x => x.Email).ToList();
            return Ok(user);
        }
        
        /// <summary>
        /// Рандомная генерация пользователей и их сообщений.
        /// </summary>
        /// <param name="countOfUsers"> Количество пользователей, которых требуется создать. </param>
        /// <param name="countOfUserMessages"> Количетво сообщений от каждого пользователя каждому пользователю. </param>
        /// <returns></returns>
        [HttpPost("random-generate")]
        public IActionResult RandomGenetare([FromQuery] int countOfUsers, [FromQuery] int countOfUserMessages)
        {
            for (int i = 0; i < countOfUsers; ++i)
            {
                string finalString;
                while (true)
                {
                    finalString = GenerateRandomString(8);
                    var sameEmail = _users.Where(x => x.Email == $"{finalString}@hse.ru").ToList();
                    if (sameEmail.Count == 0)
                        break;
                }

                _users.Add(new User()
                {
                    UserName = GenerateRandomString(10),
                    Email = $"{finalString}@hse.ru",
                    Id = idNum++
                });
            }

            _users = _users.OrderBy(x => x.Email).ToList();
            
            foreach (var recieverUser in _users)
            {
                foreach (var senderUser in _users)
                {
                    for (int i = 0; i < countOfUserMessages; ++i)
                    {
                        var randomSubject = GenerateRandomString(10);
                        var randomText = GenerateRandomString(32);
                        Random gen = new Random();
                        DateTime start = new DateTime(1995, 1, 1);
                        int range = (DateTime.Today - start).Days;           
                        var time = start.AddDays(gen.Next(range));
                        _messages.Add(new Message()
                        {
                            Subject = randomSubject,
                            MessageText = randomText,
                            RecieverEmail = recieverUser.Email,
                            SenderEmail = senderUser.Email,
                            Time = time
                        });
                    }
                }
            }
            
            return Ok(_users);
        }
        
        /// <summary>
        /// Вывод пользователей с возможностью установки лимита и оффсета.
        /// </summary>
        /// <param name="Limit"> Количество пользователей, которое необходимо вернуть (максимальное). </param>
        /// <param name="Offset"> Порядковый номер пользователя, начиная с которого необходимо получать информацию. </param>
        /// <returns> Пользователей. </returns>
        [HttpGet("get-all-user")]
        public IActionResult GetAllUsers([FromQuery] int Limit = 0, [FromQuery] int Offset = 0)
        {
            if (Limit == 0)
                Limit = _users.Count;
            else if (Offset + Limit >= _users.Count)
                return NotFound("Пользователей слишком много");
            else if (Offset < 0 || Limit < 0)
                return NotFound("Числа должны быть неотрицательны");
            var someUsers = new List<User>();
            for (int i = Offset; i < Offset + Limit && i < _users.Count; ++i)
            {
                someUsers.Add(_users[i]);
            }

            return Ok(someUsers);
        }
        
        /// <summary>
        /// Вывод данных о пользователе с определённым email-ом.
        /// </summary>
        /// <param name="Email"> Email пользователя, которого требуется найти. </param>
        /// <returns> Данные о пользователе. </returns>
        [HttpGet("get-user-by-email")]
        public IActionResult GetUserByEmail([FromQuery] string Email)
        {
            var user = _users.Where(x => x.Email == Email).ToList();
            if (user.Count == 0)
                return NotFound($"Пользователь с Email={Email} не существует!");
            return Ok(user.First());
        }
        
        /// <summary>
        /// Получения сообщений отправленных на конкретный email или с конкретного email-а или оба условия сразу.
        /// </summary>
        /// <param name="RecieverEmail"> Email получателя. </param>
        /// <param name="SenderEmail"> Email отправителя. </param>
        /// <returns> Массив с сообщениями. </returns>
        [HttpGet("get-messages")]
        public IActionResult GetMessages([FromQuery] string RecieverEmail, [FromQuery] string SenderEmail)
        {
            if (RecieverEmail == null)
            {
                var user = _users.Where(x => x.Email == SenderEmail).ToList();
                if (user.Count == 0)
                    return NotFound($"Отправитель с Email={SenderEmail} не существует!");
                var messages = _messages.Where(x => x.SenderEmail == SenderEmail).ToList();
                return Ok(messages);
            }
            else if (SenderEmail == null)
            {
                var user = _users.Where(x => x.Email == RecieverEmail).ToList();
                if (user.Count == 0)
                    return NotFound($"Получатель с Email={RecieverEmail} не существует!");
                var messages = _messages.Where(x => x.RecieverEmail == RecieverEmail).ToList();
                return Ok(messages);
            }
            else
            {
                var user = _users.Where(x => x.Email == SenderEmail).ToList();
                if (user.Count == 0)
                    return NotFound($"Отправитель с Email={SenderEmail} не существует!");
                user = _users.Where(x => x.Email == RecieverEmail).ToList();
                if (user.Count == 0)
                    return NotFound($"Получатель с Email={RecieverEmail} не существует!");
                var messages = _messages.Where(x => x.RecieverEmail == RecieverEmail &&
                                                    x.SenderEmail == SenderEmail).ToList();
                return Ok(messages);
            }
        }
        
        /// <summary>
        /// Отправить пользователю сообщение.
        /// </summary>
        /// <param name="SenderEmail"> Email отправителя. </param>
        /// <param name="RecieverEmail"> Email получателя. </param>
        /// <param name="Subject"> Тема. </param>
        /// <param name="MessageText"> Текст сообщения. </param>
        /// <returns> Ок и сообщение. </returns>
        [HttpPost("send-message-to-user")]
        public IActionResult SendMessageToUser([FromQuery] string SenderEmail, [FromQuery] string RecieverEmail, [FromQuery] string Subject, [FromQuery] string MessageText)
        {
            var user = _users.Where(x => x.Email == SenderEmail).ToList();
            if (user.Count == 0)
                return NotFound($"Отправитель с Email={SenderEmail} не существует!");
            user = _users.Where(x => x.Email == RecieverEmail).ToList();
            if (user.Count == 0)
                return NotFound($"Получатель с Email={RecieverEmail} не существует!");
            var message = new Message()
            {
                MessageText = MessageText,
                RecieverEmail = RecieverEmail,
                SenderEmail = SenderEmail,
                Subject = Subject,
                Time = DateTime.Now
            };
            _messages.Add(message);
            return Ok(message);
        }
    }
}

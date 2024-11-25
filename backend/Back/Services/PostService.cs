// using System.Text.RegularExpressions;
// using Back.Models;
// using Npgsql;
//
// namespace back.Services;
//
// public class PostService(string connectionString)
// {
//     public List<Post> GetPosts()
//     {
//         var posts = new List<Post>();
//         using var connection = new NpgsqlConnection(connectionString);
//         connection.Open();
//         using var command = new NpgsqlCommand("SELECT * FROM api_schema.post", connection);
//         using var reader = command.ExecuteReader();
//         while (reader.Read())
//         {
//             posts.Add(new Post
//             {
//                 Id = reader.GetInt32(0),
//                 Title = reader.GetString(1),
//                 Content = reader.GetString(2),
//                 UserId = reader.GetInt32(3),
//                 CreatedAt = reader.GetDateTime(4)
//             });
//         }
//
//         return posts;
//     }
//
//     public Post GetPost(int id)
//     {
//         using var connection = new NpgsqlConnection(connectionString);
//         connection.Open();
//         using var command = new NpgsqlCommand("SELECT * FROM api_schema.post WHERE id = @id", connection);
//         command.Parameters.AddWithValue("id", id);
//         using var reader = command.ExecuteReader();
//         
//         
//         
//         
//         return reader.Read()
//             ? new Post (
//                 reader.GetInt32(0),
//                 reader.GetString(1),
//                 reader.GetString(2),
//                 reader.GetInt32(3),
//                 
//             )
//             : null;
//     }
//
//     public bool CreatePost(Post post)
//     {
//         using var connection = new NpgsqlConnection(connectionString);
//         connection.Open();
//         using var command = new NpgsqlCommand("INSERT INTO posts (title, content, user_id) VALUES (@title, @content, @user_id)", connection);
//         command.Parameters.AddWithValue("title", post.Title);
//         command.Parameters.AddWithValue("content", post.Content);
//         command.Parameters.AddWithValue("user_id", post.UserId);
//         return command.ExecuteNonQuery() == 1;
//     }
//
//     public bool UpdatePost(int id, Post post)
//     {
//         using var connection = new NpgsqlConnection(connectionString);
//         connection.Open();
//         using var command = new NpgsqlCommand("UPDATE posts SET title = @title, content = @content WHERE id = @id", connection);
//         command.Parameters.AddWithValue("title", post.Title);
//         command.Parameters.AddWithValue("content", post.Content);
//         command.Parameters.AddWithValue("id", id);
//         return command.ExecuteNonQuery() == 1;
//     }
//
//     public bool DeletePost(int id)
//     {
//         using var connection = new NpgsqlConnection
//     
// }
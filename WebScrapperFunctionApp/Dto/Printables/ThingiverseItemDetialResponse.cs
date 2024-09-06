using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebScrapperFunctionApp.Dto
{


    public class Datum
    {
        [JsonProperty("content")]
        public string Content { get; set; }
    }

    public class DefaultImage
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("added")]
        public DateTime Added { get; set; }
    }

    public class DetailsPart
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("required")]
        public string Required { get; set; }

        [JsonProperty("data")]
        public List<Datum> Data { get; set; }
    }

    public class Education
    {
        [JsonProperty("grades")]
        public List<object> Grades { get; set; }

        [JsonProperty("subjects")]
        public List<object> Subjects { get; set; }
    }

    public class EduDetailsPart
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("required")]
        public object Required { get; set; }

        [JsonProperty("save_as_component")]
        public bool SaveAsComponent { get; set; }

        [JsonProperty("template")]
        public string Template { get; set; }

        [JsonProperty("fieldname")]
        public string Fieldname { get; set; }

        [JsonProperty("default")]
        public string Default { get; set; }

        [JsonProperty("opts")]
        public object Opts { get; set; }

        [JsonProperty("label")]
        public string Label { get; set; }
    }


    public class ThingiverseItemDetialResponse
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("thumbnail")]
        public string Thumbnail { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("public_url")]
        public string PublicUrl { get; set; }

        [JsonProperty("creator")]
        public Creator Creator { get; set; }

        [JsonProperty("added")]
        public DateTime Added { get; set; }

        [JsonProperty("modified")]
        public DateTime Modified { get; set; }

        [JsonProperty("is_published")]
        public int IsPublished { get; set; }

        [JsonProperty("is_wip")]
        public int IsWip { get; set; }

        [JsonProperty("is_featured")]
        public bool IsFeatured { get; set; }

        [JsonProperty("is_nsfw")]
        public bool IsNsfw { get; set; }

        [JsonProperty("is_winner")]
        public bool IsWinner { get; set; }

        [JsonProperty("is_edu_approved")]
        public bool IsEduApproved { get; set; }

        [JsonProperty("is_printable")]
        public bool IsPrintable { get; set; }

        [JsonProperty("like_count")]
        public int LikeCount { get; set; }

        [JsonProperty("is_liked")]
        public bool IsLiked { get; set; }

        [JsonProperty("collect_count")]
        public int CollectCount { get; set; }

        [JsonProperty("is_collected")]
        public bool IsCollected { get; set; }

        [JsonProperty("comment_count")]
        public int CommentCount { get; set; }

        [JsonProperty("is_watched")]
        public bool IsWatched { get; set; }

        [JsonProperty("default_image")]
        public DefaultImage DefaultImage { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("instructions")]
        public string Instructions { get; set; }

        [JsonProperty("description_html")]
        public string DescriptionHtml { get; set; }

        [JsonProperty("instructions_html")]
        public string InstructionsHtml { get; set; }

        [JsonProperty("details")]
        public string Details { get; set; }

        [JsonProperty("details_parts")]
        public List<DetailsPart> DetailsParts { get; set; }

        [JsonProperty("edu_details")]
        public string EduDetails { get; set; }

        [JsonProperty("edu_details_parts")]
        public List<EduDetailsPart> EduDetailsParts { get; set; }

        [JsonProperty("license")]
        public string License { get; set; }

        [JsonProperty("allows_derivatives")]
        public bool AllowsDerivatives { get; set; }

        [JsonProperty("files_url")]
        public string FilesUrl { get; set; }

        [JsonProperty("images_url")]
        public string ImagesUrl { get; set; }

        [JsonProperty("likes_url")]
        public string LikesUrl { get; set; }

        [JsonProperty("ancestors_url")]
        public string AncestorsUrl { get; set; }

        [JsonProperty("derivatives_url")]
        public string DerivativesUrl { get; set; }

        [JsonProperty("tags_url")]
        public string TagsUrl { get; set; }

        [JsonProperty("categories_url")]
        public string CategoriesUrl { get; set; }

        [JsonProperty("file_count")]
        public int FileCount { get; set; }

        [JsonProperty("is_private")]
        public int IsPrivate { get; set; }

        [JsonProperty("is_purchased")]
        public int IsPurchased { get; set; }

        [JsonProperty("app_id")]
        public object AppId { get; set; }

        [JsonProperty("download_count")]
        public int DownloadCount { get; set; }

        [JsonProperty("view_count")]
        public int ViewCount { get; set; }

        [JsonProperty("education")]
        public Education Education { get; set; }

        [JsonProperty("remix_count")]
        public int RemixCount { get; set; }

        [JsonProperty("make_count")]
        public int MakeCount { get; set; }

        [JsonProperty("app_count")]
        public int AppCount { get; set; }

        [JsonProperty("root_comment_count")]
        public int RootCommentCount { get; set; }

        [JsonProperty("moderation")]
        public string Moderation { get; set; }

        [JsonProperty("is_derivative")]
        public bool IsDerivative { get; set; }

        [JsonProperty("ancestors")]
        public List<object> Ancestors { get; set; }

        [JsonProperty("type_name")]
        public string TypeName { get; set; }

        [JsonProperty("is_banned")]
        public bool IsBanned { get; set; }

        [JsonProperty("is_comments_disabled")]
        public bool IsCommentsDisabled { get; set; }

        [JsonProperty("needs_moderation")]
        public int NeedsModeration { get; set; }

        [JsonProperty("zip_data")]
        public ZipData ZipData { get; set; }
    }



}

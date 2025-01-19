using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Senparc.AI.Samples.Consoles.Samples
{
    public class BaiduResult
    {
        public Toprightcontroldata topRightControlData { get; set; }
        public Authoritativeinfo authoritativeInfo { get; set; }
        public string containerDataClick { get; set; }
        public string title { get; set; }
        public string titleUrl { get; set; }
        public string preText { get; set; }
        public bool officialFlag { get; set; }
        public string iconText { get; set; }
        public string iconClass { get; set; }
        public string titleDataClick { get; set; }
        public Source source { get; set; }
        public string leftImg { get; set; }
        public string contentText { get; set; }
        public Subtitlewithicon subtitleWithIcon { get; set; }
        public Wenkuinfo wenkuInfo { get; set; }
        public string newTimeFactorStr { get; set; }
        public Tpldata tplData { get; set; }
        public object[] subLinkArray { get; set; }
        public string searchUrl { get; set; }
        public object[] normalGallery { get; set; }
        public object[] summaryList { get; set; }
        public Titlelabelprops titleLabelProps { get; set; }
        public Fronticon frontIcon { get; set; }
        public string suntitleTranslateUrl { get; set; }
        public Ttsinfo2 ttsInfo { get; set; }
        public bool showNewSafeIcon { get; set; }
        public object[] codeCoverAry { get; set; }
        public string groupTitleText { get; set; }
        public bool isGroup { get; set; }
        public object[] structList { get; set; }
        public object[] links { get; set; }
        public Style style { get; set; }
        public string extQuery { get; set; }
        public string kbShowStyle { get; set; }
        public string kbUrl { get; set; }
        public string kbFrom { get; set; }
        public string showUrl { get; set; }
        public string toolsId { get; set; }
        public string toolsTitle { get; set; }
        public string robotsUrl { get; set; }
        public string col { get; set; }
        public string urlSign { get; set; }
        public int test { get; set; }
        public Imagedata imageData { get; set; }
    }

    public class Toprightcontroldata
    {
    }

    public class Authoritativeinfo
    {
    }

    public class Source
    {
        public string sitename { get; set; }
        public string url { get; set; }
        public string img { get; set; }
        public string toolsData { get; set; }
        public string urlSign { get; set; }
        public int order { get; set; }
        public string vicon { get; set; }
    }

    public class Subtitlewithicon
    {
        public Label label { get; set; }
    }

    public class Label
    {
    }

    public class Wenkuinfo
    {
        public int score { get; set; }
        public string page { get; set; }
    }

    public class Tpldata
    {
        public object results { get; set; }
        public Footer footer { get; set; }
        public int groupOrder { get; set; }
        public Ttsinfo ttsInfo { get; set; }
        public string isRare { get; set; }
        public int URLSIGN1 { get; set; }
        public int URLSIGN2 { get; set; }
        public string site_region { get; set; }
        public string WISENEWSITESIGN { get; set; }
        public string WISENEWSUBURLSIGN { get; set; }
        public string NOMIPNEWSITESIGN { get; set; }
        public string NOMIPNEWSUBURLSIGN { get; set; }
        public string PCNEWSITESIGN { get; set; }
        public string PCNEWSUBURLSIGN { get; set; }
        public int PageOriginCodetype { get; set; }
        public int PageOriginCodetypeV2 { get; set; }
        public string LinkFoundTime { get; set; }
        public string FactorTime { get; set; }
        public string FactorTimePrecision { get; set; }
        public string LastModTime { get; set; }
        public Field_Tags_Info field_tags_info { get; set; }
        public object[] meta_di_info { get; set; }
        public int ulangtype { get; set; }
        public Official_Struct_Abstract official_struct_abstract { get; set; }
        public string src_id { get; set; }
        public string[] trans_res_list { get; set; }
        public int ti_qu_related { get; set; }
        public string templateName { get; set; }
        public int StdStg_new { get; set; }
        public bool wise_search { get; set; }
        public string[] rewrite_info { get; set; }
        public string DispUrl { get; set; }
        public int is_valid { get; set; }
        public string brief_download { get; set; }
        public string brief_popularity { get; set; }
        public Material_Data material_data { get; set; }
        public int rtset { get; set; }
        public int newTimeFactor { get; set; }
        public int timeHighlight { get; set; }
        public string site_sign { get; set; }
        public string url_sign { get; set; }
        public Resultdata resultData { get; set; }
        public Strategybits strategybits { get; set; }
        public Strategy strategy { get; set; }
        public string source_name { get; set; }
        public string posttime { get; set; }
        public Belonging belonging { get; set; }
        public Classicinfo classicInfo { get; set; }
        public string comm_sup_summary { get; set; }
        public Templatedata templateData { get; set; }
    }

    public class Footer
    {
        public Footnote footnote { get; set; }
    }

    public class Footnote
    {
        public object source { get; set; }
    }

    public class Ttsinfo
    {
        public _0 _0 { get; set; }
    }

    public class _0
    {
        public bool supportTts { get; set; }
        public bool hasTts { get; set; }
    }

    public class Field_Tags_Info
    {
        public object[] ts_kw { get; set; }
    }

    public class Official_Struct_Abstract
    {
        public string from_flag { get; set; }
        public string office_name { get; set; }
    }

    public class Material_Data
    {
        public string material_sign1 { get; set; }
        public string material_sign2 { get; set; }
        public Material_List[] material_list { get; set; }
    }

    public class Material_List
    {
        public string key { get; set; }
        public string value { get; set; }
    }

    public class Resultdata
    {
        public Tpldata1 tplData { get; set; }
        public Resdata resData { get; set; }
        public Url_Trans_Feature url_trans_feature { get; set; }
    }

    public class Tpldata1
    {
        public object results { get; set; }
        public Footer1 footer { get; set; }
        public int groupOrder { get; set; }
        public Ttsinfo1 ttsInfo { get; set; }
    }

    public class Footer1
    {
        public Footnote1 footnote { get; set; }
    }

    public class Footnote1
    {
        public object source { get; set; }
    }

    public class Ttsinfo1
    {
        public bool supportTts { get; set; }
    }

    public class Resdata
    {
        public string tplt { get; set; }
        public string tpl_sys { get; set; }
        public string env { get; set; }
    }

    public class Url_Trans_Feature
    {
        public string struct_cont_sign { get; set; }
        public string struct_simhash { get; set; }
        public string pageclassifyv2 { get; set; }
        public string fl_domain_sign { get; set; }
        public string cont_sign { get; set; }
        public string cont_simhash { get; set; }
        public string percep { get; set; }
        public string biji_aurora_score { get; set; }
        public string comment_flag { get; set; }
        public string rts_db_level { get; set; }
        public string page_classify_v2 { get; set; }
        public string m_self_page { get; set; }
        public string m_page_type { get; set; }
        public string rtse_news_site_level { get; set; }
        public string rtse_scs_news_ernie { get; set; }
        public string rtse_news_sat_ernie { get; set; }
        public string rtse_event_hotness { get; set; }
        public string f_event_score { get; set; }
        public string f_prior_event_hotness { get; set; }
        public string spam_signal { get; set; }
        public string time_stayed { get; set; }
        public string gentime_pgtime { get; set; }
        public string day_away { get; set; }
        public string ccdb_type { get; set; }
        public string dx_basic_weight { get; set; }
        public string f_basic_wei { get; set; }
        public string f_dwelling_time { get; set; }
        public string f_quality_wei { get; set; }
        public string f_cm_exam_count { get; set; }
        public string f_cm_click_count { get; set; }
        public string topk_content_dnn { get; set; }
        public string f_cm_satisfy_count { get; set; }
        public string scs_ernie_score { get; set; }
        public string topk_score_ras { get; set; }
        public string rank_nn_ctr_score { get; set; }
        public string crmm_score { get; set; }
        public string auth_modified_by_queue { get; set; }
        public string rasdx_predict_level { get; set; }
        public string ras_aurora_score { get; set; }
        public string aurora_sat_level { get; set; }
        public string rasdx_level_mapping_score { get; set; }
        public string f_ras_rel_ernie_rank { get; set; }
        public string f_ras_scs_ernie_score { get; set; }
        public string f_ras_content_quality_score { get; set; }
        public string f_ras_doc_authority_model_score { get; set; }
        public string f_qu_freshness_score_gbdt { get; set; }
        public string f_fresh_qu_dayaway { get; set; }
        public string topic_authority_model_score_norm { get; set; }
        public string f_ras_percep_click_level { get; set; }
        public string ernie_rank_score { get; set; }
        public string shoubai_vt_v2 { get; set; }
        public string calibrated_dx_modified_by_queue { get; set; }
        public string ac_model_score { get; set; }
        public string f_freshness { get; set; }
        public string freshness_new { get; set; }
        public string authority_model_score_pure { get; set; }
        public string mf_score { get; set; }
        public string dwelling_time_wise { get; set; }
        public string dwelling_time_pc { get; set; }
        public string query_content_match_ratio { get; set; }
        public string click_match_ratio { get; set; }
        public string session_bow { get; set; }
        public string cct2 { get; set; }
        public string lps_domtime_score { get; set; }
        public string f_calibrated_basic_wei { get; set; }
        public string f_dx_level { get; set; }
        public string f_event_hotness { get; set; }
        public string f_prior_event_hotness_avg { get; set; }
        public string antispam_reweight_ratio { get; set; }
        public string f_fresh_res_force_exposure { get; set; }
        public string author_unify_sign { get; set; }
        public string quality_sub_features { get; set; }
        public string official_sub_features { get; set; }
        public string nid { get; set; }
        public string thread_id { get; set; }
        public string urlsign { get; set; }
    }

    public class Strategybits
    {
        public int OFFICIALPAGE_FLAG { get; set; }
    }

    public class Strategy
    {
        public object tempName_ori { get; set; }
        public string tempName { get; set; }
        public string type { get; set; }
        public object mapping { get; set; }
        public string tpl_sys { get; set; }
        public string module { get; set; }
    }

    public class Belonging
    {
        public string list { get; set; }
        public int No { get; set; }
        public string templateName { get; set; }
    }

    public class Classicinfo
    {
        public int source { get; set; }
        public string comeFrome { get; set; }
        public string productType { get; set; }
        public int idInSource { get; set; }
        public Urls urls { get; set; }
        public int info { get; set; }
        public string siteSign { get; set; }
        public string urlSign { get; set; }
        public int urlSignHigh32 { get; set; }
        public int urlSignLow32 { get; set; }
        public int uniUrlSignHigh32 { get; set; }
        public int uniUrlSignLow32 { get; set; }
        public long siteSignHigh32 { get; set; }
        public int siteSignLow32 { get; set; }
        public int uniSiteSignHigh32 { get; set; }
        public int uniSiteSignLow32 { get; set; }
        public int docType { get; set; }
        public string disp_place_name { get; set; }
        public string timeShow { get; set; }
        public string resDbInfo { get; set; }
        public int pageTypeIndex { get; set; }
        public object dispExt { get; set; }
        public string bdDebugInfo { get; set; }
        public string authWeight { get; set; }
        public string timeFactor { get; set; }
        public string pageType { get; set; }
        public string field { get; set; }
        public int clickWeight { get; set; }
        public int pers_dnn_weight { get; set; }
        public int langtype_v2 { get; set; }
        public object gSampleLog { get; set; }
        public int isConvCode { get; set; }
        public int authorOfficialIcon { get; set; }
        public Sortinfo sortInfo { get; set; }
        public int score_level { get; set; }
        public int click_url_sign { get; set; }
        public int click_tuichang { get; set; }
        public int time_click_weight { get; set; }
        public int general_click_weight { get; set; }
        public int business_personal_click_weight { get; set; }
        public int local_personal_click_weight { get; set; }
        public int[] personal_click_weight { get; set; }
        public int[] personal_eoff { get; set; }
        public bool need_click_adjust { get; set; }
        public int url_zdzx_on { get; set; }
        public int url_zdzx_tc { get; set; }
        public int click_pred_pos { get; set; }
        public int uap_signed_key_for_query_url { get; set; }
        public int uap_signed_key_for_site { get; set; }
        public int uap_signed_key_for_url { get; set; }
        public string di_version { get; set; }
        public Url_Trans_Feature1 url_trans_feature { get; set; }
        public int final_queue_index { get; set; }
        public int isSelected { get; set; }
        public int isMask { get; set; }
        public int maskReason { get; set; }
        public int index { get; set; }
        public int isClAs { get; set; }
        public int isClusterAs { get; set; }
        public int click_orig_pos { get; set; }
        public int click_obj_pos { get; set; }
        public bool click_no_adjust { get; set; }
        public bool click_auto_hold { get; set; }
        public bool click_force_pos { get; set; }
        public int click_time_ratio { get; set; }
        public bool click_auto_hold_orig { get; set; }
        public int idea_pos { get; set; }
        public Clk_Pos_Info clk_pos_info { get; set; }
        public int click_weight { get; set; }
        public int click_weight_orig { get; set; }
        public int click_time_weight { get; set; }
        public int click_time_level { get; set; }
        public int history_url_click { get; set; }
        public int click_weight_merged_time { get; set; }
        public int click_weight_merged_pers { get; set; }
        public int click_weight_merge { get; set; }
        public int cstra { get; set; }
        public int burstFlag { get; set; }
        public string encryptionClick { get; set; }
        public string[] strategyStr { get; set; }
        public string identifyStr { get; set; }
        public string snapshootKey { get; set; }
        public string title { get; set; }
        public string url { get; set; }
        public string urlDisplay { get; set; }
        public string urlEncoded { get; set; }
        public string lastModified { get; set; }
        public string size { get; set; }
        public string code { get; set; }
        public string summary { get; set; }
        public string ppRaw { get; set; }
        public View view { get; set; }
    }

    public class Urls
    {
        public Asurls asUrls { get; set; }
    }

    public class Asurls
    {
        public int weight { get; set; }
        public int urlno { get; set; }
        public int blockno { get; set; }
        public int suburlSign { get; set; }
        public int siteSign1 { get; set; }
        public int mixSignSiteSign { get; set; }
        public int mixSignSex { get; set; }
        public int mixSignPol { get; set; }
        public long contSign { get; set; }
        public int matchProp { get; set; }
        public int[] strategys { get; set; }
    }

    public class Sortinfo
    {
        public int srcid { get; set; }
        public int from { get; set; }
        public int weight { get; set; }
        public int index { get; set; }
        public int sort { get; set; }
        public int degree { get; set; }
        public int refresh { get; set; }
        public int stdstg { get; set; }
        public int stdstl { get; set; }
        public int trace { get; set; }
        public int multiPic { get; set; }
        public string uniUrl { get; set; }
        public int stra { get; set; }
        public int status { get; set; }
        public int daIdx { get; set; }
        public int dispPlace { get; set; }
        public int serverId { get; set; }
        public int subClass { get; set; }
        public int siteUrl { get; set; }
        public int entityType { get; set; }
        public int zhixinVer { get; set; }
        public int zhixinID { get; set; }
        public int zhixinPos { get; set; }
        public int zhixinCBN { get; set; }
        public int zhixinNess { get; set; }
        public int zhixinNoC { get; set; }
    }

    public class Url_Trans_Feature1
    {
        public string struct_cont_sign { get; set; }
        public string struct_simhash { get; set; }
        public string pageclassifyv2 { get; set; }
        public string fl_domain_sign { get; set; }
        public string cont_sign { get; set; }
        public string cont_simhash { get; set; }
        public string percep { get; set; }
        public string biji_aurora_score { get; set; }
        public string comment_flag { get; set; }
        public string rts_db_level { get; set; }
        public string page_classify_v2 { get; set; }
        public string m_self_page { get; set; }
        public string m_page_type { get; set; }
        public string rtse_news_site_level { get; set; }
        public string rtse_scs_news_ernie { get; set; }
        public string rtse_news_sat_ernie { get; set; }
        public string rtse_event_hotness { get; set; }
        public string f_event_score { get; set; }
        public string f_prior_event_hotness { get; set; }
        public string spam_signal { get; set; }
        public string time_stayed { get; set; }
        public string gentime_pgtime { get; set; }
        public string day_away { get; set; }
        public string ccdb_type { get; set; }
        public string dx_basic_weight { get; set; }
        public string f_basic_wei { get; set; }
        public string f_dwelling_time { get; set; }
        public string f_quality_wei { get; set; }
        public string f_cm_exam_count { get; set; }
        public string f_cm_click_count { get; set; }
        public string topk_content_dnn { get; set; }
        public string f_cm_satisfy_count { get; set; }
        public string scs_ernie_score { get; set; }
        public string topk_score_ras { get; set; }
        public string rank_nn_ctr_score { get; set; }
        public string crmm_score { get; set; }
        public string auth_modified_by_queue { get; set; }
        public string rasdx_predict_level { get; set; }
        public string ras_aurora_score { get; set; }
        public string aurora_sat_level { get; set; }
        public string rasdx_level_mapping_score { get; set; }
        public string f_ras_rel_ernie_rank { get; set; }
        public string f_ras_scs_ernie_score { get; set; }
        public string f_ras_content_quality_score { get; set; }
        public string f_ras_doc_authority_model_score { get; set; }
        public string f_qu_freshness_score_gbdt { get; set; }
        public string f_fresh_qu_dayaway { get; set; }
        public string topic_authority_model_score_norm { get; set; }
        public string f_ras_percep_click_level { get; set; }
        public string ernie_rank_score { get; set; }
        public string shoubai_vt_v2 { get; set; }
        public string calibrated_dx_modified_by_queue { get; set; }
        public string ac_model_score { get; set; }
        public string f_freshness { get; set; }
        public string freshness_new { get; set; }
        public string authority_model_score_pure { get; set; }
        public string mf_score { get; set; }
        public string dwelling_time_wise { get; set; }
        public string dwelling_time_pc { get; set; }
        public string query_content_match_ratio { get; set; }
        public string click_match_ratio { get; set; }
        public string session_bow { get; set; }
        public string cct2 { get; set; }
        public string lps_domtime_score { get; set; }
        public string f_calibrated_basic_wei { get; set; }
        public string f_dx_level { get; set; }
        public string f_event_hotness { get; set; }
        public string f_prior_event_hotness_avg { get; set; }
        public string antispam_reweight_ratio { get; set; }
        public string f_fresh_res_force_exposure { get; set; }
        public string author_unify_sign { get; set; }
        public string quality_sub_features { get; set; }
        public string official_sub_features { get; set; }
        public string nid { get; set; }
        public string thread_id { get; set; }
        public string urlsign { get; set; }
    }

    public class Clk_Pos_Info
    {
        public int merge_index { get; set; }
        public int merge_as_index { get; set; }
    }

    public class View
    {
        public string title { get; set; }
    }

    public class Templatedata
    {
    }

    public class Titlelabelprops
    {
    }

    public class Fronticon
    {
    }

    public class Ttsinfo2
    {
        public _01 _0 { get; set; }
    }

    public class _01
    {
        public bool supportTts { get; set; }
        public bool hasTts { get; set; }
    }

    public class Style
    {
        public string promptcontent { get; set; }
        public string promptContent { get; set; }
        public string prompttext { get; set; }
        public string promptText { get; set; }
        public string sitelink_summary { get; set; }
        public string sitelinkSummary { get; set; }
        public string sitelink_summary_last { get; set; }
        public string sitelinkSummaryLast { get; set; }
        public string grouptitle { get; set; }
        public string groupTitle { get; set; }
    }

    public class Imagedata
    {
    }

}

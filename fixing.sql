--
-- PostgreSQL database dump
--

-- Dumped from database version 17.5 (Postgres.app)
-- Dumped by pg_dump version 17.0

-- Started on 2025-08-04 14:06:07 +07

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET transaction_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- TOC entry 217 (class 1259 OID 18463)
-- Name: attendance; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.attendance (
    attendance_id integer NOT NULL,
    check_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    note text,
    user_id integer NOT NULL,
    class_session_id integer NOT NULL,
    status_id integer DEFAULT 0 NOT NULL
);


ALTER TABLE public.attendance OWNER TO postgres;

--
-- TOC entry 218 (class 1259 OID 18469)
-- Name: attendance_attendance_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.attendance_attendance_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.attendance_attendance_id_seq OWNER TO postgres;

--
-- TOC entry 4104 (class 0 OID 0)
-- Dependencies: 218
-- Name: attendance_attendance_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.attendance_attendance_id_seq OWNED BY public.attendance.attendance_id;


--
-- TOC entry 259 (class 1259 OID 18869)
-- Name: attendance_status; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.attendance_status (
    status_id integer NOT NULL,
    status_name character varying(50) NOT NULL
);


ALTER TABLE public.attendance_status OWNER TO postgres;

--
-- TOC entry 219 (class 1259 OID 18470)
-- Name: class; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.class (
    class_id integer NOT NULL,
    class_code character varying(255) NOT NULL,
    instrument_id integer NOT NULL,
    total_students integer DEFAULT 0 NOT NULL,
    current_students_count integer DEFAULT 0 NOT NULL
);


ALTER TABLE public.class OWNER TO postgres;

--
-- TOC entry 220 (class 1259 OID 18473)
-- Name: class_class_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.class_class_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.class_class_id_seq OWNER TO postgres;

--
-- TOC entry 4105 (class 0 OID 0)
-- Dependencies: 220
-- Name: class_class_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.class_class_id_seq OWNED BY public.class.class_id;


--
-- TOC entry 221 (class 1259 OID 18474)
-- Name: class_session; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.class_session (
    class_session_id integer NOT NULL,
    session_number integer,
    date date,
    day_id integer NOT NULL,
    class_id integer NOT NULL,
    time_slot_id integer NOT NULL,
    room_id integer NOT NULL
);


ALTER TABLE public.class_session OWNER TO postgres;

--
-- TOC entry 222 (class 1259 OID 18477)
-- Name: class_session_class_session_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.class_session_class_session_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.class_session_class_session_id_seq OWNER TO postgres;

--
-- TOC entry 4106 (class 0 OID 0)
-- Dependencies: 222
-- Name: class_session_class_session_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.class_session_class_session_id_seq OWNED BY public.class_session.class_session_id;


--
-- TOC entry 223 (class 1259 OID 18478)
-- Name: consultation_request; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.consultation_request (
    consultation_request_id integer NOT NULL,
    fullname character varying(255),
    contact_number text,
    email character varying(255) NOT NULL,
    note text NOT NULL,
    has_contact boolean DEFAULT false,
    statistic_id integer,
    consultation_topic_id integer,
    handled_by integer,
    handled_at timestamp with time zone
);


ALTER TABLE public.consultation_request OWNER TO postgres;

--
-- TOC entry 224 (class 1259 OID 18484)
-- Name: consultation_request_consultation_request_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.consultation_request_consultation_request_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.consultation_request_consultation_request_id_seq OWNER TO postgres;

--
-- TOC entry 4107 (class 0 OID 0)
-- Dependencies: 224
-- Name: consultation_request_consultation_request_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.consultation_request_consultation_request_id_seq OWNED BY public.consultation_request.consultation_request_id;


--
-- TOC entry 225 (class 1259 OID 18485)
-- Name: consultation_topic; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.consultation_topic (
    consultation_topic_id integer NOT NULL,
    consultation_topic_name character varying(255)
);


ALTER TABLE public.consultation_topic OWNER TO postgres;

--
-- TOC entry 226 (class 1259 OID 18488)
-- Name: consultation_topic_consultation_topic_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.consultation_topic_consultation_topic_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.consultation_topic_consultation_topic_id_seq OWNER TO postgres;

--
-- TOC entry 4108 (class 0 OID 0)
-- Dependencies: 226
-- Name: consultation_topic_consultation_topic_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.consultation_topic_consultation_topic_id_seq OWNED BY public.consultation_topic.consultation_topic_id;


--
-- TOC entry 263 (class 1259 OID 18909)
-- Name: day_of_week_lookup; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.day_of_week_lookup (
    day_of_week_id integer NOT NULL,
    day_name character varying(20) NOT NULL,
    day_number integer NOT NULL
);


ALTER TABLE public.day_of_week_lookup OWNER TO postgres;

--
-- TOC entry 262 (class 1259 OID 18908)
-- Name: day_of_week_lookup_day_of_week_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.day_of_week_lookup_day_of_week_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.day_of_week_lookup_day_of_week_id_seq OWNER TO postgres;

--
-- TOC entry 4109 (class 0 OID 0)
-- Dependencies: 262
-- Name: day_of_week_lookup_day_of_week_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.day_of_week_lookup_day_of_week_id_seq OWNED BY public.day_of_week_lookup.day_of_week_id;


--
-- TOC entry 258 (class 1259 OID 18847)
-- Name: days; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.days (
    day_id integer NOT NULL,
    week_id integer,
    date_of_day date NOT NULL,
    day_of_week_name character varying(10) NOT NULL,
    is_active boolean DEFAULT true
);


ALTER TABLE public.days OWNER TO postgres;

--
-- TOC entry 257 (class 1259 OID 18846)
-- Name: days_day_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.days_day_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.days_day_id_seq OWNER TO postgres;

--
-- TOC entry 4110 (class 0 OID 0)
-- Dependencies: 257
-- Name: days_day_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.days_day_id_seq OWNED BY public.days.day_id;


--
-- TOC entry 227 (class 1259 OID 18489)
-- Name: document; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.document (
    document_id integer NOT NULL,
    lesson integer,
    lesson_name text,
    link text NOT NULL,
    instrument_id integer NOT NULL
);


ALTER TABLE public.document OWNER TO postgres;

--
-- TOC entry 228 (class 1259 OID 18494)
-- Name: document_document_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.document_document_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.document_document_id_seq OWNER TO postgres;

--
-- TOC entry 4111 (class 0 OID 0)
-- Dependencies: 228
-- Name: document_document_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.document_document_id_seq OWNED BY public.document.document_id;


--
-- TOC entry 254 (class 1259 OID 18795)
-- Name: gender; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.gender (
    gender_id integer NOT NULL,
    gender_name character varying(50) NOT NULL
);


ALTER TABLE public.gender OWNER TO postgres;

--
-- TOC entry 253 (class 1259 OID 18794)
-- Name: gender_gender_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.gender_gender_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.gender_gender_id_seq OWNER TO postgres;

--
-- TOC entry 4112 (class 0 OID 0)
-- Dependencies: 253
-- Name: gender_gender_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.gender_gender_id_seq OWNED BY public.gender.gender_id;


--
-- TOC entry 229 (class 1259 OID 18495)
-- Name: genre; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.genre (
    genre_id integer NOT NULL,
    genre_name character varying(255)
);


ALTER TABLE public.genre OWNER TO postgres;

--
-- TOC entry 230 (class 1259 OID 18498)
-- Name: genre_genre_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.genre_genre_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.genre_genre_id_seq OWNER TO postgres;

--
-- TOC entry 4113 (class 0 OID 0)
-- Dependencies: 230
-- Name: genre_genre_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.genre_genre_id_seq OWNED BY public.genre.genre_id;


--
-- TOC entry 231 (class 1259 OID 18499)
-- Name: instrument; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.instrument (
    instrument_id integer NOT NULL,
    instrument_name character varying(255)
);


ALTER TABLE public.instrument OWNER TO postgres;

--
-- TOC entry 232 (class 1259 OID 18502)
-- Name: instrument_instrument_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.instrument_instrument_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.instrument_instrument_id_seq OWNER TO postgres;

--
-- TOC entry 4114 (class 0 OID 0)
-- Dependencies: 232
-- Name: instrument_instrument_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.instrument_instrument_id_seq OWNED BY public.instrument.instrument_id;


--
-- TOC entry 233 (class 1259 OID 18503)
-- Name: opening_schedule; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.opening_schedule (
    opening_schedule_id integer NOT NULL,
    class_code character varying(255) NOT NULL,
    opening_day date,
    end_date date,
    student_quantity integer DEFAULT 0,
    is_advanced_class boolean DEFAULT false,
    teacher_user_id integer,
    instrument_id integer DEFAULT 1 NOT NULL,
    total_sessions integer DEFAULT 0 NOT NULL
);


ALTER TABLE public.opening_schedule OWNER TO postgres;

--
-- TOC entry 234 (class 1259 OID 18510)
-- Name: opening_schedule_opening_schedule_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.opening_schedule_opening_schedule_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.opening_schedule_opening_schedule_id_seq OWNER TO postgres;

--
-- TOC entry 4115 (class 0 OID 0)
-- Dependencies: 234
-- Name: opening_schedule_opening_schedule_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.opening_schedule_opening_schedule_id_seq OWNED BY public.opening_schedule.opening_schedule_id;


--
-- TOC entry 264 (class 1259 OID 18919)
-- Name: opening_schedule_selected_days; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.opening_schedule_selected_days (
    opening_schedule_id integer NOT NULL,
    day_of_week_id integer NOT NULL
);


ALTER TABLE public.opening_schedule_selected_days OWNER TO postgres;

--
-- TOC entry 235 (class 1259 OID 18511)
-- Name: role; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.role (
    role_id integer NOT NULL,
    role_name character varying(255)
);


ALTER TABLE public.role OWNER TO postgres;

--
-- TOC entry 236 (class 1259 OID 18514)
-- Name: role_role_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.role_role_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.role_role_id_seq OWNER TO postgres;

--
-- TOC entry 4116 (class 0 OID 0)
-- Dependencies: 236
-- Name: role_role_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.role_role_id_seq OWNED BY public.role.role_id;


--
-- TOC entry 261 (class 1259 OID 18891)
-- Name: room; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.room (
    room_id integer NOT NULL,
    room_code character varying(50) NOT NULL,
    capacity integer,
    description text
);


ALTER TABLE public.room OWNER TO postgres;

--
-- TOC entry 260 (class 1259 OID 18890)
-- Name: room_room_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.room_room_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.room_room_id_seq OWNER TO postgres;

--
-- TOC entry 4117 (class 0 OID 0)
-- Dependencies: 260
-- Name: room_room_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.room_room_id_seq OWNED BY public.room.room_id;


--
-- TOC entry 237 (class 1259 OID 18515)
-- Name: schedule; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.schedule (
    schedule_id integer NOT NULL,
    month_year date,
    note text
);


ALTER TABLE public.schedule OWNER TO postgres;

--
-- TOC entry 238 (class 1259 OID 18520)
-- Name: schedule_schedule_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.schedule_schedule_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.schedule_schedule_id_seq OWNER TO postgres;

--
-- TOC entry 4118 (class 0 OID 0)
-- Dependencies: 238
-- Name: schedule_schedule_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.schedule_schedule_id_seq OWNED BY public.schedule.schedule_id;


--
-- TOC entry 239 (class 1259 OID 18521)
-- Name: sheet; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.sheet (
    sheet_id integer NOT NULL,
    sheet_url text NOT NULL
);


ALTER TABLE public.sheet OWNER TO postgres;

--
-- TOC entry 240 (class 1259 OID 18526)
-- Name: sheet_music; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.sheet_music (
    sheet_music_id integer NOT NULL,
    number integer,
    music_name character varying(255),
    composer character varying(255) NOT NULL,
    cover_url text NOT NULL,
    sheet_quantity integer,
    favorite_count integer DEFAULT 0,
    sheet_id integer
);


ALTER TABLE public.sheet_music OWNER TO postgres;

--
-- TOC entry 241 (class 1259 OID 18532)
-- Name: sheet_music_genres; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.sheet_music_genres (
    sheet_music_id integer NOT NULL,
    genre_id integer NOT NULL
);


ALTER TABLE public.sheet_music_genres OWNER TO postgres;

--
-- TOC entry 242 (class 1259 OID 18535)
-- Name: sheet_music_sheet_music_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.sheet_music_sheet_music_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.sheet_music_sheet_music_id_seq OWNER TO postgres;

--
-- TOC entry 4119 (class 0 OID 0)
-- Dependencies: 242
-- Name: sheet_music_sheet_music_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.sheet_music_sheet_music_id_seq OWNED BY public.sheet_music.sheet_music_id;


--
-- TOC entry 243 (class 1259 OID 18536)
-- Name: sheet_sheet_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.sheet_sheet_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.sheet_sheet_id_seq OWNER TO postgres;

--
-- TOC entry 4120 (class 0 OID 0)
-- Dependencies: 243
-- Name: sheet_sheet_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.sheet_sheet_id_seq OWNED BY public.sheet.sheet_id;


--
-- TOC entry 244 (class 1259 OID 18537)
-- Name: statistic; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.statistic (
    statistic_id integer NOT NULL,
    date date,
    new_students integer DEFAULT 0,
    monthly_revenue numeric(10,2) DEFAULT 0.00,
    consultation_count integer DEFAULT 0,
    total_students integer DEFAULT 0,
    consultation_request_count integer DEFAULT 0
);


ALTER TABLE public.statistic OWNER TO postgres;

--
-- TOC entry 245 (class 1259 OID 18545)
-- Name: statistic_statistic_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.statistic_statistic_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.statistic_statistic_id_seq OWNER TO postgres;

--
-- TOC entry 4121 (class 0 OID 0)
-- Dependencies: 245
-- Name: statistic_statistic_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.statistic_statistic_id_seq OWNED BY public.statistic.statistic_id;


--
-- TOC entry 246 (class 1259 OID 18546)
-- Name: timeslot; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.timeslot (
    timeslot_id integer NOT NULL,
    start_time time without time zone NOT NULL,
    end_time time without time zone NOT NULL
);


ALTER TABLE public.timeslot OWNER TO postgres;

--
-- TOC entry 247 (class 1259 OID 18549)
-- Name: timeslot_timeslot_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.timeslot_timeslot_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.timeslot_timeslot_id_seq OWNER TO postgres;

--
-- TOC entry 4122 (class 0 OID 0)
-- Dependencies: 247
-- Name: timeslot_timeslot_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.timeslot_timeslot_id_seq OWNED BY public.timeslot.timeslot_id;


--
-- TOC entry 248 (class 1259 OID 18550)
-- Name: user; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."user" (
    user_id integer NOT NULL,
    username character varying(255),
    account_name text,
    password character varying(255) NOT NULL,
    address text,
    phone_number character varying(255),
    is_disabled boolean DEFAULT false,
    create_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    avatar_url text,
    birthday date,
    role_id integer,
    statistic_id integer,
    email character varying(255),
    gender_id integer DEFAULT 3 NOT NULL
);


ALTER TABLE public."user" OWNER TO postgres;

--
-- TOC entry 249 (class 1259 OID 18557)
-- Name: user_class; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.user_class (
    class_id integer NOT NULL,
    user_id integer NOT NULL
);


ALTER TABLE public.user_class OWNER TO postgres;

--
-- TOC entry 250 (class 1259 OID 18560)
-- Name: user_doc; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.user_doc (
    user_id integer NOT NULL,
    document_id integer NOT NULL
);


ALTER TABLE public.user_doc OWNER TO postgres;

--
-- TOC entry 251 (class 1259 OID 18563)
-- Name: user_favorite_sheet; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.user_favorite_sheet (
    user_id integer NOT NULL,
    sheet_music_id integer NOT NULL,
    is_favorite boolean DEFAULT true
);


ALTER TABLE public.user_favorite_sheet OWNER TO postgres;

--
-- TOC entry 252 (class 1259 OID 18567)
-- Name: user_user_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.user_user_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.user_user_id_seq OWNER TO postgres;

--
-- TOC entry 4123 (class 0 OID 0)
-- Dependencies: 252
-- Name: user_user_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.user_user_id_seq OWNED BY public."user".user_id;


--
-- TOC entry 256 (class 1259 OID 18834)
-- Name: weeks; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.weeks (
    week_id integer NOT NULL,
    schedule_id integer NOT NULL,
    week_number_in_month integer NOT NULL,
    start_date date NOT NULL,
    end_date date NOT NULL,
    num_active_days integer DEFAULT 0
);


ALTER TABLE public.weeks OWNER TO postgres;

--
-- TOC entry 255 (class 1259 OID 18833)
-- Name: weeks_week_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.weeks_week_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.weeks_week_id_seq OWNER TO postgres;

--
-- TOC entry 4124 (class 0 OID 0)
-- Dependencies: 255
-- Name: weeks_week_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.weeks_week_id_seq OWNED BY public.weeks.week_id;


--
-- TOC entry 3749 (class 2604 OID 18572)
-- Name: attendance attendance_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.attendance ALTER COLUMN attendance_id SET DEFAULT nextval('public.attendance_attendance_id_seq'::regclass);


--
-- TOC entry 3752 (class 2604 OID 18573)
-- Name: class class_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.class ALTER COLUMN class_id SET DEFAULT nextval('public.class_class_id_seq'::regclass);


--
-- TOC entry 3755 (class 2604 OID 18574)
-- Name: class_session class_session_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.class_session ALTER COLUMN class_session_id SET DEFAULT nextval('public.class_session_class_session_id_seq'::regclass);


--
-- TOC entry 3756 (class 2604 OID 18575)
-- Name: consultation_request consultation_request_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.consultation_request ALTER COLUMN consultation_request_id SET DEFAULT nextval('public.consultation_request_consultation_request_id_seq'::regclass);


--
-- TOC entry 3758 (class 2604 OID 18576)
-- Name: consultation_topic consultation_topic_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.consultation_topic ALTER COLUMN consultation_topic_id SET DEFAULT nextval('public.consultation_topic_consultation_topic_id_seq'::regclass);


--
-- TOC entry 3790 (class 2604 OID 18912)
-- Name: day_of_week_lookup day_of_week_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.day_of_week_lookup ALTER COLUMN day_of_week_id SET DEFAULT nextval('public.day_of_week_lookup_day_of_week_id_seq'::regclass);


--
-- TOC entry 3787 (class 2604 OID 18850)
-- Name: days day_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.days ALTER COLUMN day_id SET DEFAULT nextval('public.days_day_id_seq'::regclass);


--
-- TOC entry 3759 (class 2604 OID 18577)
-- Name: document document_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.document ALTER COLUMN document_id SET DEFAULT nextval('public.document_document_id_seq'::regclass);


--
-- TOC entry 3784 (class 2604 OID 18798)
-- Name: gender gender_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.gender ALTER COLUMN gender_id SET DEFAULT nextval('public.gender_gender_id_seq'::regclass);


--
-- TOC entry 3760 (class 2604 OID 18578)
-- Name: genre genre_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.genre ALTER COLUMN genre_id SET DEFAULT nextval('public.genre_genre_id_seq'::regclass);


--
-- TOC entry 3761 (class 2604 OID 18579)
-- Name: instrument instrument_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.instrument ALTER COLUMN instrument_id SET DEFAULT nextval('public.instrument_instrument_id_seq'::regclass);


--
-- TOC entry 3762 (class 2604 OID 18580)
-- Name: opening_schedule opening_schedule_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.opening_schedule ALTER COLUMN opening_schedule_id SET DEFAULT nextval('public.opening_schedule_opening_schedule_id_seq'::regclass);


--
-- TOC entry 3767 (class 2604 OID 18581)
-- Name: role role_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.role ALTER COLUMN role_id SET DEFAULT nextval('public.role_role_id_seq'::regclass);


--
-- TOC entry 3789 (class 2604 OID 18894)
-- Name: room room_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.room ALTER COLUMN room_id SET DEFAULT nextval('public.room_room_id_seq'::regclass);


--
-- TOC entry 3768 (class 2604 OID 18582)
-- Name: schedule schedule_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.schedule ALTER COLUMN schedule_id SET DEFAULT nextval('public.schedule_schedule_id_seq'::regclass);


--
-- TOC entry 3769 (class 2604 OID 18583)
-- Name: sheet sheet_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.sheet ALTER COLUMN sheet_id SET DEFAULT nextval('public.sheet_sheet_id_seq'::regclass);


--
-- TOC entry 3770 (class 2604 OID 18584)
-- Name: sheet_music sheet_music_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.sheet_music ALTER COLUMN sheet_music_id SET DEFAULT nextval('public.sheet_music_sheet_music_id_seq'::regclass);


--
-- TOC entry 3772 (class 2604 OID 18585)
-- Name: statistic statistic_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.statistic ALTER COLUMN statistic_id SET DEFAULT nextval('public.statistic_statistic_id_seq'::regclass);


--
-- TOC entry 3778 (class 2604 OID 18586)
-- Name: timeslot timeslot_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.timeslot ALTER COLUMN timeslot_id SET DEFAULT nextval('public.timeslot_timeslot_id_seq'::regclass);


--
-- TOC entry 3779 (class 2604 OID 18587)
-- Name: user user_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."user" ALTER COLUMN user_id SET DEFAULT nextval('public.user_user_id_seq'::regclass);


--
-- TOC entry 3785 (class 2604 OID 18837)
-- Name: weeks week_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.weeks ALTER COLUMN week_id SET DEFAULT nextval('public.weeks_week_id_seq'::regclass);


--
-- TOC entry 4051 (class 0 OID 18463)
-- Dependencies: 217
-- Data for Name: attendance; Type: TABLE DATA; Schema: public; Owner: postgres
--



--
-- TOC entry 4093 (class 0 OID 18869)
-- Dependencies: 259
-- Data for Name: attendance_status; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.attendance_status (status_id, status_name) VALUES (1, 'Present');
INSERT INTO public.attendance_status (status_id, status_name) VALUES (2, 'Absent');
INSERT INTO public.attendance_status (status_id, status_name) VALUES (0, 'Unmarked');


--
-- TOC entry 4053 (class 0 OID 18470)
-- Dependencies: 219
-- Data for Name: class; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.class (class_id, class_code, instrument_id, total_students, current_students_count) VALUES (3, 'ADV160', 2, 0, 0);
INSERT INTO public.class (class_id, class_code, instrument_id, total_students, current_students_count) VALUES (5, 'ARR130', 2, 0, 0);
INSERT INTO public.class (class_id, class_code, instrument_id, total_students, current_students_count) VALUES (4, 'AB125', 1, 0, 0);
INSERT INTO public.class (class_id, class_code, instrument_id, total_students, current_students_count) VALUES (6, 'string', 1, 0, 0);
INSERT INTO public.class (class_id, class_code, instrument_id, total_students, current_students_count) VALUES (7, 'NN-412', 1, 0, 0);


--
-- TOC entry 4055 (class 0 OID 18474)
-- Dependencies: 221
-- Data for Name: class_session; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.class_session (class_session_id, session_number, date, day_id, class_id, time_slot_id, room_id) VALUES (1, 1, '2025-07-31', 31, 6, 2, 2);
INSERT INTO public.class_session (class_session_id, session_number, date, day_id, class_id, time_slot_id, room_id) VALUES (2, 2, '2025-08-04', 35, 6, 2, 2);
INSERT INTO public.class_session (class_session_id, session_number, date, day_id, class_id, time_slot_id, room_id) VALUES (3, 3, '2025-08-07', 38, 6, 2, 2);
INSERT INTO public.class_session (class_session_id, session_number, date, day_id, class_id, time_slot_id, room_id) VALUES (4, 4, '2025-08-11', 42, 6, 2, 2);
INSERT INTO public.class_session (class_session_id, session_number, date, day_id, class_id, time_slot_id, room_id) VALUES (5, 1, '2025-07-31', 31, 7, 2, 2);
INSERT INTO public.class_session (class_session_id, session_number, date, day_id, class_id, time_slot_id, room_id) VALUES (6, 2, '2025-08-04', 35, 7, 2, 2);
INSERT INTO public.class_session (class_session_id, session_number, date, day_id, class_id, time_slot_id, room_id) VALUES (7, 3, '2025-08-07', 38, 7, 2, 2);
INSERT INTO public.class_session (class_session_id, session_number, date, day_id, class_id, time_slot_id, room_id) VALUES (8, 4, '2025-08-11', 42, 7, 2, 2);


--
-- TOC entry 4057 (class 0 OID 18478)
-- Dependencies: 223
-- Data for Name: consultation_request; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.consultation_request (consultation_request_id, fullname, contact_number, email, note, has_contact, statistic_id, consultation_topic_id, handled_by, handled_at) VALUES (1, 'Toi', '111111111', 'toi@gmail.com', '', true, NULL, 1, 1, '2025-07-08 23:07:56.15768+07');


--
-- TOC entry 4059 (class 0 OID 18485)
-- Dependencies: 225
-- Data for Name: consultation_topic; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.consultation_topic (consultation_topic_id, consultation_topic_name) VALUES (1, 'Guitar');
INSERT INTO public.consultation_topic (consultation_topic_id, consultation_topic_name) VALUES (2, 'Piano');


--
-- TOC entry 4097 (class 0 OID 18909)
-- Dependencies: 263
-- Data for Name: day_of_week_lookup; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.day_of_week_lookup (day_of_week_id, day_name, day_number) VALUES (1, 'Monday', 1);
INSERT INTO public.day_of_week_lookup (day_of_week_id, day_name, day_number) VALUES (2, 'Tuesday', 2);
INSERT INTO public.day_of_week_lookup (day_of_week_id, day_name, day_number) VALUES (3, 'Wednesday', 3);
INSERT INTO public.day_of_week_lookup (day_of_week_id, day_name, day_number) VALUES (4, 'Thursday', 4);
INSERT INTO public.day_of_week_lookup (day_of_week_id, day_name, day_number) VALUES (5, 'Friday', 5);
INSERT INTO public.day_of_week_lookup (day_of_week_id, day_name, day_number) VALUES (6, 'Saturday', 6);
INSERT INTO public.day_of_week_lookup (day_of_week_id, day_name, day_number) VALUES (7, 'Sunday', 7);


--
-- TOC entry 4092 (class 0 OID 18847)
-- Dependencies: 258
-- Data for Name: days; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (1, 1, '2025-07-01', 'Tuesday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (2, 1, '2025-07-02', 'Wednesday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (3, 1, '2025-07-03', 'Thursday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (4, 1, '2025-07-04', 'Friday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (5, 1, '2025-07-05', 'Saturday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (6, 1, '2025-07-06', 'Sunday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (7, 1, '2025-07-07', 'Monday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (8, 2, '2025-07-08', 'Tuesday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (9, 2, '2025-07-09', 'Wednesday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (10, 2, '2025-07-10', 'Thursday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (11, 2, '2025-07-11', 'Friday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (12, 2, '2025-07-12', 'Saturday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (13, 2, '2025-07-13', 'Sunday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (14, 2, '2025-07-14', 'Monday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (15, 3, '2025-07-15', 'Tuesday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (16, 3, '2025-07-16', 'Wednesday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (17, 3, '2025-07-17', 'Thursday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (18, 3, '2025-07-18', 'Friday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (19, 3, '2025-07-19', 'Saturday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (20, 3, '2025-07-20', 'Sunday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (21, 3, '2025-07-21', 'Monday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (22, 4, '2025-07-22', 'Tuesday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (23, 4, '2025-07-23', 'Wednesday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (24, 4, '2025-07-24', 'Thursday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (25, 4, '2025-07-25', 'Friday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (26, 4, '2025-07-26', 'Saturday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (27, 4, '2025-07-27', 'Sunday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (28, 4, '2025-07-28', 'Monday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (29, 5, '2025-07-29', 'Tuesday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (30, 5, '2025-07-30', 'Wednesday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (31, 5, '2025-07-31', 'Thursday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (32, 6, '2025-08-01', 'Friday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (33, 6, '2025-08-02', 'Saturday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (34, 6, '2025-08-03', 'Sunday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (35, 6, '2025-08-04', 'Monday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (36, 6, '2025-08-05', 'Tuesday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (37, 6, '2025-08-06', 'Wednesday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (38, 6, '2025-08-07', 'Thursday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (39, 7, '2025-08-08', 'Friday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (40, 7, '2025-08-09', 'Saturday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (41, 7, '2025-08-10', 'Sunday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (42, 7, '2025-08-11', 'Monday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (43, 7, '2025-08-12', 'Tuesday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (44, 7, '2025-08-13', 'Wednesday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (45, 7, '2025-08-14', 'Thursday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (46, 8, '2025-08-15', 'Friday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (47, 8, '2025-08-16', 'Saturday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (48, 8, '2025-08-17', 'Sunday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (49, 8, '2025-08-18', 'Monday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (50, 8, '2025-08-19', 'Tuesday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (51, 8, '2025-08-20', 'Wednesday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (52, 8, '2025-08-21', 'Thursday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (53, 9, '2025-08-22', 'Friday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (54, 9, '2025-08-23', 'Saturday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (55, 9, '2025-08-24', 'Sunday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (56, 9, '2025-08-25', 'Monday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (57, 9, '2025-08-26', 'Tuesday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (58, 9, '2025-08-27', 'Wednesday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (59, 9, '2025-08-28', 'Thursday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (60, 10, '2025-08-29', 'Friday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (61, 10, '2025-08-30', 'Saturday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (62, 10, '2025-08-31', 'Sunday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (63, 11, '2025-09-01', 'Monday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (64, 11, '2025-09-02', 'Tuesday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (65, 11, '2025-09-03', 'Wednesday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (66, 11, '2025-09-04', 'Thursday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (67, 11, '2025-09-05', 'Friday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (68, 11, '2025-09-06', 'Saturday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (69, 11, '2025-09-07', 'Sunday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (70, 12, '2025-09-08', 'Monday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (71, 12, '2025-09-09', 'Tuesday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (72, 12, '2025-09-10', 'Wednesday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (73, 12, '2025-09-11', 'Thursday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (74, 12, '2025-09-12', 'Friday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (75, 12, '2025-09-13', 'Saturday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (76, 12, '2025-09-14', 'Sunday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (77, 13, '2025-09-15', 'Monday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (78, 13, '2025-09-16', 'Tuesday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (79, 13, '2025-09-17', 'Wednesday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (80, 13, '2025-09-18', 'Thursday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (81, 13, '2025-09-19', 'Friday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (82, 13, '2025-09-20', 'Saturday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (83, 13, '2025-09-21', 'Sunday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (84, 14, '2025-09-22', 'Monday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (85, 14, '2025-09-23', 'Tuesday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (86, 14, '2025-09-24', 'Wednesday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (87, 14, '2025-09-25', 'Thursday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (88, 14, '2025-09-26', 'Friday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (89, 14, '2025-09-27', 'Saturday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (90, 14, '2025-09-28', 'Sunday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (91, 15, '2025-09-29', 'Monday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (92, 15, '2025-09-30', 'Tuesday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (93, 16, '2025-10-01', 'Wednesday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (94, 16, '2025-10-02', 'Thursday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (95, 16, '2025-10-03', 'Friday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (96, 16, '2025-10-04', 'Saturday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (97, 16, '2025-10-05', 'Sunday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (98, 16, '2025-10-06', 'Monday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (99, 16, '2025-10-07', 'Tuesday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (100, 17, '2025-10-08', 'Wednesday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (101, 17, '2025-10-09', 'Thursday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (102, 17, '2025-10-10', 'Friday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (103, 17, '2025-10-11', 'Saturday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (104, 17, '2025-10-12', 'Sunday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (105, 17, '2025-10-13', 'Monday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (106, 17, '2025-10-14', 'Tuesday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (107, 18, '2025-10-15', 'Wednesday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (108, 18, '2025-10-16', 'Thursday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (109, 18, '2025-10-17', 'Friday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (110, 18, '2025-10-18', 'Saturday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (111, 18, '2025-10-19', 'Sunday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (112, 18, '2025-10-20', 'Monday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (113, 18, '2025-10-21', 'Tuesday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (114, 19, '2025-10-22', 'Wednesday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (115, 19, '2025-10-23', 'Thursday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (116, 19, '2025-10-24', 'Friday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (117, 19, '2025-10-25', 'Saturday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (118, 19, '2025-10-26', 'Sunday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (119, 19, '2025-10-27', 'Monday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (120, 19, '2025-10-28', 'Tuesday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (121, 20, '2025-10-29', 'Wednesday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (122, 20, '2025-10-30', 'Thursday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (123, 20, '2025-10-31', 'Friday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (124, 21, '2025-11-01', 'Saturday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (125, 21, '2025-11-02', 'Sunday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (126, 21, '2025-11-03', 'Monday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (127, 21, '2025-11-04', 'Tuesday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (128, 21, '2025-11-05', 'Wednesday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (129, 21, '2025-11-06', 'Thursday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (130, 21, '2025-11-07', 'Friday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (131, 22, '2025-11-08', 'Saturday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (132, 22, '2025-11-09', 'Sunday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (133, 22, '2025-11-10', 'Monday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (134, 22, '2025-11-11', 'Tuesday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (135, 22, '2025-11-12', 'Wednesday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (136, 22, '2025-11-13', 'Thursday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (137, 22, '2025-11-14', 'Friday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (138, 23, '2025-11-15', 'Saturday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (139, 23, '2025-11-16', 'Sunday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (140, 23, '2025-11-17', 'Monday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (141, 23, '2025-11-18', 'Tuesday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (142, 23, '2025-11-19', 'Wednesday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (143, 23, '2025-11-20', 'Thursday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (144, 23, '2025-11-21', 'Friday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (145, 24, '2025-11-22', 'Saturday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (146, 24, '2025-11-23', 'Sunday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (147, 24, '2025-11-24', 'Monday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (148, 24, '2025-11-25', 'Tuesday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (149, 24, '2025-11-26', 'Wednesday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (150, 24, '2025-11-27', 'Thursday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (151, 24, '2025-11-28', 'Friday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (152, 25, '2025-11-29', 'Saturday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (153, 25, '2025-11-30', 'Sunday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (154, 26, '2025-12-01', 'Monday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (155, 26, '2025-12-02', 'Tuesday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (156, 26, '2025-12-03', 'Wednesday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (157, 26, '2025-12-04', 'Thursday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (158, 26, '2025-12-05', 'Friday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (159, 26, '2025-12-06', 'Saturday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (160, 26, '2025-12-07', 'Sunday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (161, 27, '2025-12-08', 'Monday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (162, 27, '2025-12-09', 'Tuesday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (163, 27, '2025-12-10', 'Wednesday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (164, 27, '2025-12-11', 'Thursday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (165, 27, '2025-12-12', 'Friday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (166, 27, '2025-12-13', 'Saturday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (167, 27, '2025-12-14', 'Sunday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (168, 28, '2025-12-15', 'Monday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (169, 28, '2025-12-16', 'Tuesday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (170, 28, '2025-12-17', 'Wednesday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (171, 28, '2025-12-18', 'Thursday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (172, 28, '2025-12-19', 'Friday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (173, 28, '2025-12-20', 'Saturday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (174, 28, '2025-12-21', 'Sunday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (175, 29, '2025-12-22', 'Monday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (176, 29, '2025-12-23', 'Tuesday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (177, 29, '2025-12-24', 'Wednesday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (178, 29, '2025-12-25', 'Thursday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (179, 29, '2025-12-26', 'Friday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (180, 29, '2025-12-27', 'Saturday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (181, 29, '2025-12-28', 'Sunday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (182, 30, '2025-12-29', 'Monday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (183, 30, '2025-12-30', 'Tuesday', true);
INSERT INTO public.days (day_id, week_id, date_of_day, day_of_week_name, is_active) VALUES (184, 30, '2025-12-31', 'Wednesday', true);


--
-- TOC entry 4061 (class 0 OID 18489)
-- Dependencies: 227
-- Data for Name: document; Type: TABLE DATA; Schema: public; Owner: postgres
--



--
-- TOC entry 4088 (class 0 OID 18795)
-- Dependencies: 254
-- Data for Name: gender; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.gender (gender_id, gender_name) VALUES (1, 'Nam');
INSERT INTO public.gender (gender_id, gender_name) VALUES (2, 'Nữ');
INSERT INTO public.gender (gender_id, gender_name) VALUES (3, 'Khác');


--
-- TOC entry 4063 (class 0 OID 18495)
-- Dependencies: 229
-- Data for Name: genre; Type: TABLE DATA; Schema: public; Owner: postgres
--



--
-- TOC entry 4065 (class 0 OID 18499)
-- Dependencies: 231
-- Data for Name: instrument; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.instrument (instrument_id, instrument_name) VALUES (1, 'Guitar');
INSERT INTO public.instrument (instrument_id, instrument_name) VALUES (2, 'Piano');


--
-- TOC entry 4067 (class 0 OID 18503)
-- Dependencies: 233
-- Data for Name: opening_schedule; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.opening_schedule (opening_schedule_id, class_code, opening_day, end_date, student_quantity, is_advanced_class, teacher_user_id, instrument_id, total_sessions) VALUES (1, 'ADV160', '2025-10-08', '2025-11-08', 30, false, NULL, 1, 0);
INSERT INTO public.opening_schedule (opening_schedule_id, class_code, opening_day, end_date, student_quantity, is_advanced_class, teacher_user_id, instrument_id, total_sessions) VALUES (3, 'AB125', '2025-08-08', '2025-09-08', 30, false, 2, 1, 0);
INSERT INTO public.opening_schedule (opening_schedule_id, class_code, opening_day, end_date, student_quantity, is_advanced_class, teacher_user_id, instrument_id, total_sessions) VALUES (4, 'ARR130', '2025-11-13', '2025-12-13', 12, false, 2, 2, 0);
INSERT INTO public.opening_schedule (opening_schedule_id, class_code, opening_day, end_date, student_quantity, is_advanced_class, teacher_user_id, instrument_id, total_sessions) VALUES (5, 'string', '2025-07-30', '2025-08-30', 20, true, 2, 1, 4);
INSERT INTO public.opening_schedule (opening_schedule_id, class_code, opening_day, end_date, student_quantity, is_advanced_class, teacher_user_id, instrument_id, total_sessions) VALUES (6, 'NN-412', '2025-07-30', '2025-08-30', 20, true, 2, 1, 4);


--
-- TOC entry 4098 (class 0 OID 18919)
-- Dependencies: 264
-- Data for Name: opening_schedule_selected_days; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.opening_schedule_selected_days (opening_schedule_id, day_of_week_id) VALUES (5, 1);
INSERT INTO public.opening_schedule_selected_days (opening_schedule_id, day_of_week_id) VALUES (5, 4);
INSERT INTO public.opening_schedule_selected_days (opening_schedule_id, day_of_week_id) VALUES (6, 1);
INSERT INTO public.opening_schedule_selected_days (opening_schedule_id, day_of_week_id) VALUES (6, 4);


--
-- TOC entry 4069 (class 0 OID 18511)
-- Dependencies: 235
-- Data for Name: role; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.role (role_id, role_name) VALUES (1, 'Admin');
INSERT INTO public.role (role_id, role_name) VALUES (2, 'Teacher');
INSERT INTO public.role (role_id, role_name) VALUES (3, 'Student');


--
-- TOC entry 4095 (class 0 OID 18891)
-- Dependencies: 261
-- Data for Name: room; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.room (room_id, room_code, capacity, description) VALUES (2, 'A-40', 40, '');


--
-- TOC entry 4071 (class 0 OID 18515)
-- Dependencies: 237
-- Data for Name: schedule; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.schedule (schedule_id, month_year, note) VALUES (1, '2025-07-01', 'Tự động tạo cho tháng 7/2025');
INSERT INTO public.schedule (schedule_id, month_year, note) VALUES (2, '2025-08-01', 'Tự động tạo cho tháng 8/2025');
INSERT INTO public.schedule (schedule_id, month_year, note) VALUES (3, '2025-09-01', 'Tự động tạo cho tháng 9/2025');
INSERT INTO public.schedule (schedule_id, month_year, note) VALUES (4, '2025-10-01', 'Tự động tạo cho tháng 10/2025');
INSERT INTO public.schedule (schedule_id, month_year, note) VALUES (5, '2025-11-01', 'Tự động tạo cho tháng 11/2025');
INSERT INTO public.schedule (schedule_id, month_year, note) VALUES (6, '2025-12-01', 'Tự động tạo cho tháng 12/2025');


--
-- TOC entry 4073 (class 0 OID 18521)
-- Dependencies: 239
-- Data for Name: sheet; Type: TABLE DATA; Schema: public; Owner: postgres
--



--
-- TOC entry 4074 (class 0 OID 18526)
-- Dependencies: 240
-- Data for Name: sheet_music; Type: TABLE DATA; Schema: public; Owner: postgres
--



--
-- TOC entry 4075 (class 0 OID 18532)
-- Dependencies: 241
-- Data for Name: sheet_music_genres; Type: TABLE DATA; Schema: public; Owner: postgres
--



--
-- TOC entry 4078 (class 0 OID 18537)
-- Dependencies: 244
-- Data for Name: statistic; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.statistic (statistic_id, date, new_students, monthly_revenue, consultation_count, total_students, consultation_request_count) VALUES (1, '2025-07-01', 0, 0.00, 1, 0, 1);


--
-- TOC entry 4080 (class 0 OID 18546)
-- Dependencies: 246
-- Data for Name: timeslot; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.timeslot (timeslot_id, start_time, end_time) VALUES (1, '07:00:00', '08:30:00');
INSERT INTO public.timeslot (timeslot_id, start_time, end_time) VALUES (2, '09:00:00', '10:30:00');
INSERT INTO public.timeslot (timeslot_id, start_time, end_time) VALUES (3, '14:00:00', '15:30:00');
INSERT INTO public.timeslot (timeslot_id, start_time, end_time) VALUES (4, '16:00:00', '17:30:00');
INSERT INTO public.timeslot (timeslot_id, start_time, end_time) VALUES (5, '19:00:00', '20:30:00');


--
-- TOC entry 4082 (class 0 OID 18550)
-- Dependencies: 248
-- Data for Name: user; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public."user" (user_id, username, account_name, password, address, phone_number, is_disabled, create_at, avatar_url, birthday, role_id, statistic_id, email, gender_id) VALUES (1, 'user1', 'Admin', 'password123', 'HCMC', '0999888777', false, '2025-07-08 08:39:48.6525', 'https://daoxuanlong.blob.core.windows.net/innovus/avatars/0c645dd4-6296-4c0b-be35-44a6954f9a66-kanye.webp', '2020-07-08', 1, NULL, NULL, 3);
INSERT INTO public."user" (user_id, username, account_name, password, address, phone_number, is_disabled, create_at, avatar_url, birthday, role_id, statistic_id, email, gender_id) VALUES (2, 'user2', 'Teacher', 'password123', 'HCMC', '0777888999', false, '2025-07-08 08:41:50.497163', 'https://daoxuanlong.blob.core.windows.net/innovus/avatars/499cd4c6-b27d-422d-adf9-aea1ccd0d2d2-ab67616d00001e02d9194aa18fa4c9362b47464f.jpeg', '1999-07-08', 2, NULL, NULL, 3);
INSERT INTO public."user" (user_id, username, account_name, password, address, phone_number, is_disabled, create_at, avatar_url, birthday, role_id, statistic_id, email, gender_id) VALUES (3, 'user3', 'StudentABC', 'password123', 'HCMC', '0999888111', false, '2025-07-23 22:00:23.648201', 'https://daoxuanlong.blob.core.windows.net/innovus/avatars/47b47099-2e4c-49ba-b6b0-a2c2d784fb10-trav.jpeg', '1997-07-23', 3, NULL, 'user3@example.com', 1);


--
-- TOC entry 4083 (class 0 OID 18557)
-- Dependencies: 249
-- Data for Name: user_class; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.user_class (class_id, user_id) VALUES (4, 3);


--
-- TOC entry 4084 (class 0 OID 18560)
-- Dependencies: 250
-- Data for Name: user_doc; Type: TABLE DATA; Schema: public; Owner: postgres
--



--
-- TOC entry 4085 (class 0 OID 18563)
-- Dependencies: 251
-- Data for Name: user_favorite_sheet; Type: TABLE DATA; Schema: public; Owner: postgres
--



--
-- TOC entry 4090 (class 0 OID 18834)
-- Dependencies: 256
-- Data for Name: weeks; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.weeks (week_id, schedule_id, week_number_in_month, start_date, end_date, num_active_days) VALUES (1, 1, 1, '2025-07-01', '2025-07-07', 7);
INSERT INTO public.weeks (week_id, schedule_id, week_number_in_month, start_date, end_date, num_active_days) VALUES (2, 1, 2, '2025-07-08', '2025-07-14', 7);
INSERT INTO public.weeks (week_id, schedule_id, week_number_in_month, start_date, end_date, num_active_days) VALUES (3, 1, 3, '2025-07-15', '2025-07-21', 7);
INSERT INTO public.weeks (week_id, schedule_id, week_number_in_month, start_date, end_date, num_active_days) VALUES (4, 1, 4, '2025-07-22', '2025-07-28', 7);
INSERT INTO public.weeks (week_id, schedule_id, week_number_in_month, start_date, end_date, num_active_days) VALUES (5, 1, 5, '2025-07-29', '2025-07-31', 3);
INSERT INTO public.weeks (week_id, schedule_id, week_number_in_month, start_date, end_date, num_active_days) VALUES (6, 2, 1, '2025-08-01', '2025-08-07', 7);
INSERT INTO public.weeks (week_id, schedule_id, week_number_in_month, start_date, end_date, num_active_days) VALUES (7, 2, 2, '2025-08-08', '2025-08-14', 7);
INSERT INTO public.weeks (week_id, schedule_id, week_number_in_month, start_date, end_date, num_active_days) VALUES (8, 2, 3, '2025-08-15', '2025-08-21', 7);
INSERT INTO public.weeks (week_id, schedule_id, week_number_in_month, start_date, end_date, num_active_days) VALUES (9, 2, 4, '2025-08-22', '2025-08-28', 7);
INSERT INTO public.weeks (week_id, schedule_id, week_number_in_month, start_date, end_date, num_active_days) VALUES (10, 2, 5, '2025-08-29', '2025-08-31', 3);
INSERT INTO public.weeks (week_id, schedule_id, week_number_in_month, start_date, end_date, num_active_days) VALUES (11, 3, 1, '2025-09-01', '2025-09-07', 7);
INSERT INTO public.weeks (week_id, schedule_id, week_number_in_month, start_date, end_date, num_active_days) VALUES (12, 3, 2, '2025-09-08', '2025-09-14', 7);
INSERT INTO public.weeks (week_id, schedule_id, week_number_in_month, start_date, end_date, num_active_days) VALUES (13, 3, 3, '2025-09-15', '2025-09-21', 7);
INSERT INTO public.weeks (week_id, schedule_id, week_number_in_month, start_date, end_date, num_active_days) VALUES (14, 3, 4, '2025-09-22', '2025-09-28', 7);
INSERT INTO public.weeks (week_id, schedule_id, week_number_in_month, start_date, end_date, num_active_days) VALUES (15, 3, 5, '2025-09-29', '2025-09-30', 2);
INSERT INTO public.weeks (week_id, schedule_id, week_number_in_month, start_date, end_date, num_active_days) VALUES (16, 4, 1, '2025-10-01', '2025-10-07', 7);
INSERT INTO public.weeks (week_id, schedule_id, week_number_in_month, start_date, end_date, num_active_days) VALUES (17, 4, 2, '2025-10-08', '2025-10-14', 7);
INSERT INTO public.weeks (week_id, schedule_id, week_number_in_month, start_date, end_date, num_active_days) VALUES (18, 4, 3, '2025-10-15', '2025-10-21', 7);
INSERT INTO public.weeks (week_id, schedule_id, week_number_in_month, start_date, end_date, num_active_days) VALUES (19, 4, 4, '2025-10-22', '2025-10-28', 7);
INSERT INTO public.weeks (week_id, schedule_id, week_number_in_month, start_date, end_date, num_active_days) VALUES (20, 4, 5, '2025-10-29', '2025-10-31', 3);
INSERT INTO public.weeks (week_id, schedule_id, week_number_in_month, start_date, end_date, num_active_days) VALUES (21, 5, 1, '2025-11-01', '2025-11-07', 7);
INSERT INTO public.weeks (week_id, schedule_id, week_number_in_month, start_date, end_date, num_active_days) VALUES (22, 5, 2, '2025-11-08', '2025-11-14', 7);
INSERT INTO public.weeks (week_id, schedule_id, week_number_in_month, start_date, end_date, num_active_days) VALUES (23, 5, 3, '2025-11-15', '2025-11-21', 7);
INSERT INTO public.weeks (week_id, schedule_id, week_number_in_month, start_date, end_date, num_active_days) VALUES (24, 5, 4, '2025-11-22', '2025-11-28', 7);
INSERT INTO public.weeks (week_id, schedule_id, week_number_in_month, start_date, end_date, num_active_days) VALUES (25, 5, 5, '2025-11-29', '2025-11-30', 2);
INSERT INTO public.weeks (week_id, schedule_id, week_number_in_month, start_date, end_date, num_active_days) VALUES (26, 6, 1, '2025-12-01', '2025-12-07', 7);
INSERT INTO public.weeks (week_id, schedule_id, week_number_in_month, start_date, end_date, num_active_days) VALUES (27, 6, 2, '2025-12-08', '2025-12-14', 7);
INSERT INTO public.weeks (week_id, schedule_id, week_number_in_month, start_date, end_date, num_active_days) VALUES (28, 6, 3, '2025-12-15', '2025-12-21', 7);
INSERT INTO public.weeks (week_id, schedule_id, week_number_in_month, start_date, end_date, num_active_days) VALUES (29, 6, 4, '2025-12-22', '2025-12-28', 7);
INSERT INTO public.weeks (week_id, schedule_id, week_number_in_month, start_date, end_date, num_active_days) VALUES (30, 6, 5, '2025-12-29', '2025-12-31', 3);


--
-- TOC entry 4125 (class 0 OID 0)
-- Dependencies: 218
-- Name: attendance_attendance_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.attendance_attendance_id_seq', 1, false);


--
-- TOC entry 4126 (class 0 OID 0)
-- Dependencies: 220
-- Name: class_class_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.class_class_id_seq', 7, true);


--
-- TOC entry 4127 (class 0 OID 0)
-- Dependencies: 222
-- Name: class_session_class_session_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.class_session_class_session_id_seq', 8, true);


--
-- TOC entry 4128 (class 0 OID 0)
-- Dependencies: 224
-- Name: consultation_request_consultation_request_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.consultation_request_consultation_request_id_seq', 1, true);


--
-- TOC entry 4129 (class 0 OID 0)
-- Dependencies: 226
-- Name: consultation_topic_consultation_topic_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.consultation_topic_consultation_topic_id_seq', 2, true);


--
-- TOC entry 4130 (class 0 OID 0)
-- Dependencies: 262
-- Name: day_of_week_lookup_day_of_week_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.day_of_week_lookup_day_of_week_id_seq', 7, true);


--
-- TOC entry 4131 (class 0 OID 0)
-- Dependencies: 257
-- Name: days_day_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.days_day_id_seq', 184, true);


--
-- TOC entry 4132 (class 0 OID 0)
-- Dependencies: 228
-- Name: document_document_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.document_document_id_seq', 1, false);


--
-- TOC entry 4133 (class 0 OID 0)
-- Dependencies: 253
-- Name: gender_gender_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.gender_gender_id_seq', 3, true);


--
-- TOC entry 4134 (class 0 OID 0)
-- Dependencies: 230
-- Name: genre_genre_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.genre_genre_id_seq', 1, false);


--
-- TOC entry 4135 (class 0 OID 0)
-- Dependencies: 232
-- Name: instrument_instrument_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.instrument_instrument_id_seq', 2, true);


--
-- TOC entry 4136 (class 0 OID 0)
-- Dependencies: 234
-- Name: opening_schedule_opening_schedule_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.opening_schedule_opening_schedule_id_seq', 6, true);


--
-- TOC entry 4137 (class 0 OID 0)
-- Dependencies: 236
-- Name: role_role_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.role_role_id_seq', 3, true);


--
-- TOC entry 4138 (class 0 OID 0)
-- Dependencies: 260
-- Name: room_room_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.room_room_id_seq', 2, true);


--
-- TOC entry 4139 (class 0 OID 0)
-- Dependencies: 238
-- Name: schedule_schedule_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.schedule_schedule_id_seq', 6, true);


--
-- TOC entry 4140 (class 0 OID 0)
-- Dependencies: 242
-- Name: sheet_music_sheet_music_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.sheet_music_sheet_music_id_seq', 1, false);


--
-- TOC entry 4141 (class 0 OID 0)
-- Dependencies: 243
-- Name: sheet_sheet_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.sheet_sheet_id_seq', 1, false);


--
-- TOC entry 4142 (class 0 OID 0)
-- Dependencies: 245
-- Name: statistic_statistic_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.statistic_statistic_id_seq', 1, true);


--
-- TOC entry 4143 (class 0 OID 0)
-- Dependencies: 247
-- Name: timeslot_timeslot_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.timeslot_timeslot_id_seq', 5, true);


--
-- TOC entry 4144 (class 0 OID 0)
-- Dependencies: 252
-- Name: user_user_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.user_user_id_seq', 3, true);


--
-- TOC entry 4145 (class 0 OID 0)
-- Dependencies: 255
-- Name: weeks_week_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.weeks_week_id_seq', 30, true);


--
-- TOC entry 3792 (class 2606 OID 18590)
-- Name: attendance attendance_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.attendance
    ADD CONSTRAINT attendance_pkey PRIMARY KEY (attendance_id);


--
-- TOC entry 3860 (class 2606 OID 18873)
-- Name: attendance_status attendance_status_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.attendance_status
    ADD CONSTRAINT attendance_status_pkey PRIMARY KEY (status_id);


--
-- TOC entry 3862 (class 2606 OID 18875)
-- Name: attendance_status attendance_status_status_name_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.attendance_status
    ADD CONSTRAINT attendance_status_status_name_key UNIQUE (status_name);


--
-- TOC entry 3796 (class 2606 OID 18592)
-- Name: class class_class_code_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.class
    ADD CONSTRAINT class_class_code_key UNIQUE (class_code);


--
-- TOC entry 3798 (class 2606 OID 18594)
-- Name: class class_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.class
    ADD CONSTRAINT class_pkey PRIMARY KEY (class_id);


--
-- TOC entry 3800 (class 2606 OID 18596)
-- Name: class_session class_session_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.class_session
    ADD CONSTRAINT class_session_pkey PRIMARY KEY (class_session_id);


--
-- TOC entry 3802 (class 2606 OID 18598)
-- Name: consultation_request consultation_request_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.consultation_request
    ADD CONSTRAINT consultation_request_pkey PRIMARY KEY (consultation_request_id);


--
-- TOC entry 3804 (class 2606 OID 18600)
-- Name: consultation_topic consultation_topic_consultation_topic_name_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.consultation_topic
    ADD CONSTRAINT consultation_topic_consultation_topic_name_key UNIQUE (consultation_topic_name);


--
-- TOC entry 3806 (class 2606 OID 18602)
-- Name: consultation_topic consultation_topic_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.consultation_topic
    ADD CONSTRAINT consultation_topic_pkey PRIMARY KEY (consultation_topic_id);


--
-- TOC entry 3868 (class 2606 OID 18916)
-- Name: day_of_week_lookup day_of_week_lookup_day_name_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.day_of_week_lookup
    ADD CONSTRAINT day_of_week_lookup_day_name_key UNIQUE (day_name);


--
-- TOC entry 3870 (class 2606 OID 18918)
-- Name: day_of_week_lookup day_of_week_lookup_day_number_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.day_of_week_lookup
    ADD CONSTRAINT day_of_week_lookup_day_number_key UNIQUE (day_number);


--
-- TOC entry 3872 (class 2606 OID 18914)
-- Name: day_of_week_lookup day_of_week_lookup_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.day_of_week_lookup
    ADD CONSTRAINT day_of_week_lookup_pkey PRIMARY KEY (day_of_week_id);


--
-- TOC entry 3858 (class 2606 OID 18853)
-- Name: days days_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.days
    ADD CONSTRAINT days_pkey PRIMARY KEY (day_id);


--
-- TOC entry 3808 (class 2606 OID 18604)
-- Name: document document_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.document
    ADD CONSTRAINT document_pkey PRIMARY KEY (document_id);


--
-- TOC entry 3852 (class 2606 OID 18802)
-- Name: gender gender_gender_name_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.gender
    ADD CONSTRAINT gender_gender_name_key UNIQUE (gender_name);


--
-- TOC entry 3854 (class 2606 OID 18800)
-- Name: gender gender_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.gender
    ADD CONSTRAINT gender_pkey PRIMARY KEY (gender_id);


--
-- TOC entry 3810 (class 2606 OID 18606)
-- Name: genre genre_genre_name_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.genre
    ADD CONSTRAINT genre_genre_name_key UNIQUE (genre_name);


--
-- TOC entry 3812 (class 2606 OID 18608)
-- Name: genre genre_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.genre
    ADD CONSTRAINT genre_pkey PRIMARY KEY (genre_id);


--
-- TOC entry 3814 (class 2606 OID 18610)
-- Name: instrument instrument_instrument_name_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.instrument
    ADD CONSTRAINT instrument_instrument_name_key UNIQUE (instrument_name);


--
-- TOC entry 3816 (class 2606 OID 18612)
-- Name: instrument instrument_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.instrument
    ADD CONSTRAINT instrument_pkey PRIMARY KEY (instrument_id);


--
-- TOC entry 3818 (class 2606 OID 18616)
-- Name: opening_schedule opening_schedule_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.opening_schedule
    ADD CONSTRAINT opening_schedule_pkey PRIMARY KEY (opening_schedule_id);


--
-- TOC entry 3874 (class 2606 OID 18923)
-- Name: opening_schedule_selected_days opening_schedule_selected_days_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.opening_schedule_selected_days
    ADD CONSTRAINT opening_schedule_selected_days_pkey PRIMARY KEY (opening_schedule_id, day_of_week_id);


--
-- TOC entry 3820 (class 2606 OID 18618)
-- Name: role role_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.role
    ADD CONSTRAINT role_pkey PRIMARY KEY (role_id);


--
-- TOC entry 3822 (class 2606 OID 18620)
-- Name: role role_role_name_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.role
    ADD CONSTRAINT role_role_name_key UNIQUE (role_name);


--
-- TOC entry 3864 (class 2606 OID 18898)
-- Name: room room_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.room
    ADD CONSTRAINT room_pkey PRIMARY KEY (room_id);


--
-- TOC entry 3866 (class 2606 OID 18900)
-- Name: room room_room_code_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.room
    ADD CONSTRAINT room_room_code_key UNIQUE (room_code);


--
-- TOC entry 3824 (class 2606 OID 18622)
-- Name: schedule schedule_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.schedule
    ADD CONSTRAINT schedule_pkey PRIMARY KEY (schedule_id);


--
-- TOC entry 3832 (class 2606 OID 18624)
-- Name: sheet_music_genres sheet_music_genres_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.sheet_music_genres
    ADD CONSTRAINT sheet_music_genres_pkey PRIMARY KEY (sheet_music_id, genre_id);


--
-- TOC entry 3828 (class 2606 OID 18626)
-- Name: sheet_music sheet_music_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.sheet_music
    ADD CONSTRAINT sheet_music_pkey PRIMARY KEY (sheet_music_id);


--
-- TOC entry 3830 (class 2606 OID 18628)
-- Name: sheet_music sheet_music_sheet_id_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.sheet_music
    ADD CONSTRAINT sheet_music_sheet_id_key UNIQUE (sheet_id);


--
-- TOC entry 3826 (class 2606 OID 18630)
-- Name: sheet sheet_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.sheet
    ADD CONSTRAINT sheet_pkey PRIMARY KEY (sheet_id);


--
-- TOC entry 3834 (class 2606 OID 18632)
-- Name: statistic statistic_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.statistic
    ADD CONSTRAINT statistic_pkey PRIMARY KEY (statistic_id);


--
-- TOC entry 3836 (class 2606 OID 18634)
-- Name: timeslot timeslot_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.timeslot
    ADD CONSTRAINT timeslot_pkey PRIMARY KEY (timeslot_id);


--
-- TOC entry 3838 (class 2606 OID 18636)
-- Name: timeslot timeslot_start_time_end_time_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.timeslot
    ADD CONSTRAINT timeslot_start_time_end_time_key UNIQUE (start_time, end_time);


--
-- TOC entry 3794 (class 2606 OID 18883)
-- Name: attendance uq_user_class_session; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.attendance
    ADD CONSTRAINT uq_user_class_session UNIQUE (user_id, class_session_id);


--
-- TOC entry 3846 (class 2606 OID 18638)
-- Name: user_class user_class_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.user_class
    ADD CONSTRAINT user_class_pkey PRIMARY KEY (class_id, user_id);


--
-- TOC entry 3848 (class 2606 OID 18640)
-- Name: user_doc user_doc_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.user_doc
    ADD CONSTRAINT user_doc_pkey PRIMARY KEY (user_id, document_id);


--
-- TOC entry 3840 (class 2606 OID 18804)
-- Name: user user_email_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."user"
    ADD CONSTRAINT user_email_key UNIQUE (email);


--
-- TOC entry 3850 (class 2606 OID 18642)
-- Name: user_favorite_sheet user_favorite_sheet_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.user_favorite_sheet
    ADD CONSTRAINT user_favorite_sheet_pkey PRIMARY KEY (user_id, sheet_music_id);


--
-- TOC entry 3842 (class 2606 OID 18644)
-- Name: user user_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."user"
    ADD CONSTRAINT user_pkey PRIMARY KEY (user_id);


--
-- TOC entry 3844 (class 2606 OID 18648)
-- Name: user user_username_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."user"
    ADD CONSTRAINT user_username_key UNIQUE (username);


--
-- TOC entry 3856 (class 2606 OID 18840)
-- Name: weeks weeks_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.weeks
    ADD CONSTRAINT weeks_pkey PRIMARY KEY (week_id);


--
-- TOC entry 3875 (class 2606 OID 18651)
-- Name: attendance fk_attendance_class_session; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.attendance
    ADD CONSTRAINT fk_attendance_class_session FOREIGN KEY (class_session_id) REFERENCES public.class_session(class_session_id) ON UPDATE CASCADE ON DELETE CASCADE;


--
-- TOC entry 3876 (class 2606 OID 18884)
-- Name: attendance fk_attendance_status_id; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.attendance
    ADD CONSTRAINT fk_attendance_status_id FOREIGN KEY (status_id) REFERENCES public.attendance_status(status_id) ON DELETE RESTRICT;


--
-- TOC entry 3877 (class 2606 OID 18656)
-- Name: attendance fk_attendance_user; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.attendance
    ADD CONSTRAINT fk_attendance_user FOREIGN KEY (user_id) REFERENCES public."user"(user_id) ON UPDATE CASCADE ON DELETE CASCADE;


--
-- TOC entry 3878 (class 2606 OID 18771)
-- Name: class fk_class_instrument; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.class
    ADD CONSTRAINT fk_class_instrument FOREIGN KEY (instrument_id) REFERENCES public.instrument(instrument_id) ON UPDATE CASCADE ON DELETE RESTRICT;


--
-- TOC entry 3879 (class 2606 OID 18661)
-- Name: class_session fk_class_session_class; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.class_session
    ADD CONSTRAINT fk_class_session_class FOREIGN KEY (class_id) REFERENCES public.class(class_id) ON UPDATE CASCADE ON DELETE RESTRICT;


--
-- TOC entry 3880 (class 2606 OID 18864)
-- Name: class_session fk_class_session_days; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.class_session
    ADD CONSTRAINT fk_class_session_days FOREIGN KEY (day_id) REFERENCES public.days(day_id) ON UPDATE CASCADE ON DELETE CASCADE;


--
-- TOC entry 3881 (class 2606 OID 18901)
-- Name: class_session fk_class_session_room; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.class_session
    ADD CONSTRAINT fk_class_session_room FOREIGN KEY (room_id) REFERENCES public.room(room_id) ON UPDATE CASCADE ON DELETE RESTRICT;


--
-- TOC entry 3882 (class 2606 OID 18666)
-- Name: class_session fk_class_session_timeslot; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.class_session
    ADD CONSTRAINT fk_class_session_timeslot FOREIGN KEY (time_slot_id) REFERENCES public.timeslot(timeslot_id) ON UPDATE CASCADE ON DELETE RESTRICT;


--
-- TOC entry 3883 (class 2606 OID 18766)
-- Name: consultation_request fk_consultation_request_handled_by; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.consultation_request
    ADD CONSTRAINT fk_consultation_request_handled_by FOREIGN KEY (handled_by) REFERENCES public."user"(user_id) ON UPDATE CASCADE ON DELETE SET NULL;


--
-- TOC entry 3884 (class 2606 OID 18676)
-- Name: consultation_request fk_consultation_request_statistic; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.consultation_request
    ADD CONSTRAINT fk_consultation_request_statistic FOREIGN KEY (statistic_id) REFERENCES public.statistic(statistic_id) ON UPDATE CASCADE ON DELETE SET NULL;


--
-- TOC entry 3885 (class 2606 OID 18681)
-- Name: consultation_request fk_consultation_request_topic; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.consultation_request
    ADD CONSTRAINT fk_consultation_request_topic FOREIGN KEY (consultation_topic_id) REFERENCES public.consultation_topic(consultation_topic_id) ON UPDATE CASCADE ON DELETE RESTRICT;


--
-- TOC entry 3903 (class 2606 OID 18854)
-- Name: days fk_days_weeks; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.days
    ADD CONSTRAINT fk_days_weeks FOREIGN KEY (week_id) REFERENCES public.weeks(week_id) ON UPDATE CASCADE ON DELETE SET NULL;


--
-- TOC entry 3886 (class 2606 OID 18686)
-- Name: document fk_document_instrument; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.document
    ADD CONSTRAINT fk_document_instrument FOREIGN KEY (instrument_id) REFERENCES public.instrument(instrument_id) ON UPDATE CASCADE ON DELETE RESTRICT;


--
-- TOC entry 3887 (class 2606 OID 18939)
-- Name: opening_schedule fk_opening_schedule_class; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.opening_schedule
    ADD CONSTRAINT fk_opening_schedule_class FOREIGN KEY (class_code) REFERENCES public.class(class_code) ON UPDATE CASCADE ON DELETE RESTRICT;


--
-- TOC entry 3888 (class 2606 OID 18789)
-- Name: opening_schedule fk_opening_schedule_instrument; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.opening_schedule
    ADD CONSTRAINT fk_opening_schedule_instrument FOREIGN KEY (instrument_id) REFERENCES public.instrument(instrument_id) ON UPDATE CASCADE ON DELETE RESTRICT;


--
-- TOC entry 3889 (class 2606 OID 18761)
-- Name: opening_schedule fk_opening_schedule_teacher_user; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.opening_schedule
    ADD CONSTRAINT fk_opening_schedule_teacher_user FOREIGN KEY (teacher_user_id) REFERENCES public."user"(user_id) ON UPDATE CASCADE ON DELETE SET NULL;


--
-- TOC entry 3904 (class 2606 OID 18929)
-- Name: opening_schedule_selected_days fk_os_selected_days_day_of_week; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.opening_schedule_selected_days
    ADD CONSTRAINT fk_os_selected_days_day_of_week FOREIGN KEY (day_of_week_id) REFERENCES public.day_of_week_lookup(day_of_week_id) ON UPDATE CASCADE ON DELETE RESTRICT;


--
-- TOC entry 3905 (class 2606 OID 18924)
-- Name: opening_schedule_selected_days fk_os_selected_days_opening_schedule; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.opening_schedule_selected_days
    ADD CONSTRAINT fk_os_selected_days_opening_schedule FOREIGN KEY (opening_schedule_id) REFERENCES public.opening_schedule(opening_schedule_id) ON UPDATE CASCADE ON DELETE CASCADE;


--
-- TOC entry 3891 (class 2606 OID 18691)
-- Name: sheet_music_genres fk_sheet_music_genres_genre; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.sheet_music_genres
    ADD CONSTRAINT fk_sheet_music_genres_genre FOREIGN KEY (genre_id) REFERENCES public.genre(genre_id) ON UPDATE CASCADE ON DELETE CASCADE;


--
-- TOC entry 3892 (class 2606 OID 18696)
-- Name: sheet_music_genres fk_sheet_music_genres_sheet_music; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.sheet_music_genres
    ADD CONSTRAINT fk_sheet_music_genres_sheet_music FOREIGN KEY (sheet_music_id) REFERENCES public.sheet_music(sheet_music_id) ON UPDATE CASCADE ON DELETE CASCADE;


--
-- TOC entry 3890 (class 2606 OID 18701)
-- Name: sheet_music fk_sheet_music_sheet; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.sheet_music
    ADD CONSTRAINT fk_sheet_music_sheet FOREIGN KEY (sheet_id) REFERENCES public.sheet(sheet_id) ON UPDATE CASCADE ON DELETE SET NULL;


--
-- TOC entry 3896 (class 2606 OID 18706)
-- Name: user_class fk_user_class_class; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.user_class
    ADD CONSTRAINT fk_user_class_class FOREIGN KEY (class_id) REFERENCES public.class(class_id) ON UPDATE CASCADE ON DELETE CASCADE;


--
-- TOC entry 3897 (class 2606 OID 18711)
-- Name: user_class fk_user_class_user; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.user_class
    ADD CONSTRAINT fk_user_class_user FOREIGN KEY (user_id) REFERENCES public."user"(user_id) ON UPDATE CASCADE ON DELETE CASCADE;


--
-- TOC entry 3898 (class 2606 OID 18716)
-- Name: user_doc fk_user_doc_document; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.user_doc
    ADD CONSTRAINT fk_user_doc_document FOREIGN KEY (document_id) REFERENCES public.document(document_id) ON UPDATE CASCADE ON DELETE CASCADE;


--
-- TOC entry 3899 (class 2606 OID 18721)
-- Name: user_doc fk_user_doc_user; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.user_doc
    ADD CONSTRAINT fk_user_doc_user FOREIGN KEY (user_id) REFERENCES public."user"(user_id) ON UPDATE CASCADE ON DELETE CASCADE;


--
-- TOC entry 3900 (class 2606 OID 18726)
-- Name: user_favorite_sheet fk_user_favorite_sheet_sheet_music; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.user_favorite_sheet
    ADD CONSTRAINT fk_user_favorite_sheet_sheet_music FOREIGN KEY (sheet_music_id) REFERENCES public.sheet_music(sheet_music_id) ON UPDATE CASCADE ON DELETE CASCADE;


--
-- TOC entry 3901 (class 2606 OID 18731)
-- Name: user_favorite_sheet fk_user_favorite_sheet_user; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.user_favorite_sheet
    ADD CONSTRAINT fk_user_favorite_sheet_user FOREIGN KEY (user_id) REFERENCES public."user"(user_id) ON UPDATE CASCADE ON DELETE CASCADE;


--
-- TOC entry 3893 (class 2606 OID 18806)
-- Name: user fk_user_gender; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."user"
    ADD CONSTRAINT fk_user_gender FOREIGN KEY (gender_id) REFERENCES public.gender(gender_id) ON UPDATE CASCADE ON DELETE RESTRICT;


--
-- TOC entry 3894 (class 2606 OID 18741)
-- Name: user fk_user_role; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."user"
    ADD CONSTRAINT fk_user_role FOREIGN KEY (role_id) REFERENCES public.role(role_id) ON UPDATE CASCADE ON DELETE RESTRICT;


--
-- TOC entry 3895 (class 2606 OID 18751)
-- Name: user fk_user_statistic; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."user"
    ADD CONSTRAINT fk_user_statistic FOREIGN KEY (statistic_id) REFERENCES public.statistic(statistic_id) ON UPDATE CASCADE ON DELETE SET NULL;


--
-- TOC entry 3902 (class 2606 OID 18841)
-- Name: weeks fk_weeks_schedule; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.weeks
    ADD CONSTRAINT fk_weeks_schedule FOREIGN KEY (schedule_id) REFERENCES public.schedule(schedule_id) ON UPDATE CASCADE ON DELETE CASCADE;


-- Completed on 2025-08-04 14:06:07 +07

--
-- PostgreSQL database dump complete
--


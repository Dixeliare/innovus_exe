--
-- PostgreSQL database dump
--

-- Dumped from database version 17.5 (Postgres.app)
-- Dumped by pg_dump version 17.0

-- Started on 2025-06-03 16:14:54 +07

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
-- TOC entry 254 (class 1259 OID 17689)
-- Name: attendance; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.attendance (
    attendance_id integer NOT NULL,
    status boolean,
    check_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    note text,
    user_id integer NOT NULL,
    class_session_id integer NOT NULL
);


ALTER TABLE public.attendance OWNER TO postgres;

--
-- TOC entry 253 (class 1259 OID 17688)
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
-- TOC entry 3824 (class 0 OID 0)
-- Name: attendance_attendance_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.attendance_attendance_id_seq OWNED BY public.attendance.attendance_id;


--
-- TOC entry 256 (class 1259 OID 17726)
-- Name: class_session; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.class_session (
    class_session_id integer NOT NULL,
    classroom text,
    session_date date,
    session_time time without time zone,
    subject text,
    week_id integer NOT NULL
);


ALTER TABLE public.class_session OWNER TO postgres;

--
-- TOC entry 255 (class 1259 OID 17725)
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
-- TOC entry 3825 (class 0 OID 0)
-- Name: class_session_class_session_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.class_session_class_session_id_seq OWNED BY public.class_session.class_session_id;


--
-- TOC entry 250 (class 1259 OID 17652)
-- Name: opening_schedule; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.opening_schedule (
    opening_schedule_id integer NOT NULL,
    date_start date,
    date_end date,
    time_start time without time zone,
    time_end time without time zone
);


ALTER TABLE public.opening_schedule OWNER TO postgres;

--
-- TOC entry 249 (class 1259 OID 17651)
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
-- TOC entry 3826 (class 0 OID 0)
-- Name: opening_schedule_opening_schedule_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.opening_schedule_opening_schedule_id_seq OWNED BY public.opening_schedule.opening_schedule_id;


--
-- TOC entry 248 (class 1259 OID 17646)
-- Name: role; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.role (
    role_id integer NOT NULL,
    role_name text
);


ALTER TABLE public.role OWNER TO postgres;

--
-- TOC entry 247 (class 1259 OID 17645)
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
-- TOC entry 3827 (class 0 OID 0)
-- Name: role_role_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.role_role_id_seq OWNED BY public.role.role_id;


--
-- TOC entry 252 (class 1259 OID 17657)
-- Name: schedule; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.schedule (
    schedule_id integer NOT NULL,
    schedule_name text,
    week_id integer NOT NULL
);


ALTER TABLE public.schedule OWNER TO postgres;

--
-- TOC entry 251 (class 1259 OID 17656)
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
-- TOC entry 3828 (class 0 OID 0)
-- Name: schedule_schedule_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.schedule_schedule_id_seq OWNED BY public.schedule.schedule_id;


--
-- TOC entry 246 (class 1259 OID 17637)
-- Name: statistic; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.statistic (
    statistic_id integer NOT NULL,
    total_score integer,
    rank integer,
    avg_score double precision
);


ALTER TABLE public.statistic OWNER TO postgres;

--
-- TOC entry 245 (class 1259 OID 17636)
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
-- TOC entry 3829 (class 0 OID 0)
-- Name: statistic_statistic_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.statistic_statistic_id_seq OWNED BY public.statistic.statistic_id;


--
-- TOC entry 244 (class 1259 OID 17628)
-- Name: user; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."user" (
    user_id integer NOT NULL,
    user_name text,
    email text,
    password_hash text,
    statistic_id integer,
    role_id integer NOT NULL,
    schedule_id integer,
    opening_schedule_id integer
);


ALTER TABLE public."user" OWNER TO postgres;

--
-- TOC entry 243 (class 1259 OID 17627)
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
-- TOC entry 3830 (class 0 OID 0)
-- Name: user_user_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.user_user_id_seq OWNED BY public."user".user_id;


--
-- TOC entry 258 (class 1259 OID 17734)
-- Name: week; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.week (
    week_id integer NOT NULL,
    week_name text,
    start_date date,
    end_date date
);


ALTER TABLE public.week OWNER TO postgres;

--
-- TOC entry 257 (class 1259 OID 17733)
-- Name: week_week_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.week_week_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.week_week_id_seq OWNER TO postgres;

--
-- TOC entry 3831 (class 0 OID 0)
-- Name: week_week_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.week_week_id_seq OWNED BY public.week.week_id;


--
-- TOC entry 3656 (class 2604 OID 17692)
-- Name: attendance attendance_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.attendance ALTER COLUMN attendance_id SET DEFAULT nextval('public.attendance_attendance_id_seq'::regclass);


--
-- TOC entry 3657 (class 2604 OID 17729)
-- Name: class_session class_session_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.class_session ALTER COLUMN class_session_id SET DEFAULT nextval('public.class_session_class_session_id_seq'::regclass);


--
-- TOC entry 3654 (class 2604 OID 17655)
-- Name: opening_schedule opening_schedule_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.opening_schedule ALTER COLUMN opening_schedule_id SET DEFAULT nextval('public.opening_schedule_opening_schedule_id_seq'::regclass);


--
-- TOC entry 3653 (class 2604 OID 17649)
-- Name: role role_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.role ALTER COLUMN role_id SET DEFAULT nextval('public.role_role_id_seq'::regclass);


--
-- TOC entry 3655 (class 2604 OID 17660)
-- Name: schedule schedule_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.schedule ALTER COLUMN schedule_id SET DEFAULT nextval('public.schedule_schedule_id_seq'::regclass);


--
-- TOC entry 3652 (class 2604 OID 17640)
-- Name: statistic statistic_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.statistic ALTER COLUMN statistic_id SET DEFAULT nextval('public.statistic_statistic_id_seq'::regclass);


--
-- TOC entry 3651 (class 2604 OID 17631)
-- Name: user user_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."user" ALTER COLUMN user_id SET DEFAULT nextval('public.user_user_id_seq'::regclass);


--
-- TOC entry 3658 (class 2604 OID 17737)
-- Name: week week_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.week ALTER COLUMN week_id SET DEFAULT nextval('public.week_week_id_seq'::regclass);


--
-- TOC entry 3804 (class 2606 OID 17694)
-- Name: attendance attendance_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.attendance
    ADD CONSTRAINT attendance_pkey PRIMARY KEY (attendance_id);


--
-- TOC entry 3806 (class 2606 OID 17731)
-- Name: class_session class_session_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.class_session
    ADD CONSTRAINT class_session_pkey PRIMARY KEY (class_session_id);


--
-- TOC entry 3798 (class 2606 OID 17654)
-- Name: opening_schedule opening_schedule_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.opening_schedule
    ADD CONSTRAINT opening_schedule_pkey PRIMARY KEY (opening_schedule_id);


--
-- TOC entry 3796 (class 2606 OID 17650)
-- Name: role role_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.role
    ADD CONSTRAINT role_pkey PRIMARY KEY (role_id);


--
-- TOC entry 3800 (class 2606 OID 17662)
-- Name: schedule schedule_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.schedule
    ADD CONSTRAINT schedule_pkey PRIMARY KEY (schedule_id);


--
-- TOC entry 3794 (class 2606 OID 17642)
-- Name: statistic statistic_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.statistic
    ADD CONSTRAINT statistic_pkey PRIMARY KEY (statistic_id);


--
-- TOC entry 3790 (class 2606 OID 17635)
-- Name: user user_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."user"
    ADD CONSTRAINT user_pkey PRIMARY KEY (user_id);


--
-- TOC entry 3792 (class 2606 OID 17644)
-- Name: user user_user_name_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."user"
    ADD CONSTRAINT user_user_name_key UNIQUE (user_name);


--
-- TOC entry 3808 (class 2606 OID 17739)
-- Name: week week_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.week
    ADD CONSTRAINT week_pkey PRIMARY KEY (week_id);


--
-- TOC entry 3810 (class 2606 OID 17695)
-- Name: attendance fk_attendance_class_session; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.attendance
    ADD CONSTRAINT fk_attendance_class_session FOREIGN KEY (class_session_id) REFERENCES public.class_session(class_session_id) ON UPDATE CASCADE ON DELETE CASCADE;


--
-- TOC entry 3811 (class 2606 OID 17700)
-- Name: attendance fk_attendance_user; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.attendance
    ADD CONSTRAINT fk_attendance_user FOREIGN KEY (user_id) REFERENCES public."user"(user_id) ON UPDATE CASCADE ON DELETE CASCADE;


--
-- TOC entry 3817 (class 2606 OID 17740)
-- Name: class_session fk_class_session_week; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.class_session
    ADD CONSTRAINT fk_class_session_week FOREIGN KEY (week_id) REFERENCES public.week(week_id) ON UPDATE CASCADE ON DELETE CASCADE;


--
-- TOC entry 3812 (class 2606 OID 17718)
-- Name: user fk_user_opening_schedule; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."user"
    ADD CONSTRAINT fk_user_opening_schedule FOREIGN KEY (opening_schedule_id) REFERENCES public.opening_schedule(opening_schedule_id) ON UPDATE CASCADE ON DELETE SET NULL;


--
-- TOC entry 3814 (class 2606 OID 17708)
-- Name: user fk_user_role; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."user"
    ADD CONSTRAINT fk_user_role FOREIGN KEY (role_id) REFERENCES public.role(role_id) ON UPDATE CASCADE ON DELETE RESTRICT;


--
-- TOC entry 3815 (class 2606 OID 17723)
-- Name: user fk_user_schedule_personal; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."user"
    ADD CONSTRAINT fk_user_schedule_personal FOREIGN KEY (schedule_id) REFERENCES public.schedule(schedule_id) ON UPDATE CASCADE ON DELETE SET NULL;


--
-- TOC entry 3816 (class 2606 OID 17713)
-- Name: user fk_user_statistic; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."user"
    ADD CONSTRAINT fk_user_statistic FOREIGN KEY (statistic_id) REFERENCES public.statistic(statistic_id) ON UPDATE CASCADE ON DELETE SET NULL;


--
-- TOC entry 3829 (class 2606 OID 17661)
-- Name: week fk_week_schedule; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.week
    ADD CONSTRAINT fk_week_schedule FOREIGN KEY (schedule_id) REFERENCES public.schedule(schedule_id) ON UPDATE CASCADE ON DELETE RESTRICT;


--
-- Data for Name: role; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.role (role_id, role_name) VALUES
    (1, 'Admin'),
    (2, 'Student');


--
-- Data for Name: statistic; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.statistic (statistic_id, total_score, rank, avg_score) VALUES
    (1, 950, 1, 9.5),
    (2, 880, 2, 8.8);


--
-- Data for Name: opening_schedule; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.opening_schedule (opening_schedule_id, date_start, date_end, time_start, time_end) VALUES
    (1, '2025-01-01', '2025-12-31', '08:00:00', '17:00:00'),
    (2, '2025-02-01', '2025-11-30', '09:00:00', '18:00:00');


--
-- Data for Name: user; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public."user" (user_id, user_name, email, password_hash, statistic_id, role_id, schedule_id, opening_schedule_id) VALUES
    (1, 'admin_user', 'admin@example.com', 'hashedpassword_admin_123', 1, 1, NULL, 1),
    (2, 'student_user', 'student@example.com', 'hashedpassword_student_456', 2, 2, NULL, 2);


--
-- Data for Name: week; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.week (week_id, week_name, start_date, end_date) VALUES
    (1, 'Week 1', '2025-01-06', '2025-01-12'),
    (2, 'Week 2', '2025-01-13', '2025-01-19');


--
-- Data for Name: schedule; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.schedule (schedule_id, schedule_name, week_id) VALUES
    (1, 'Morning Classes', 1),
    (2, 'Afternoon Workshops', 2);


--
-- Data for Name: class_session; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.class_session (class_session_id, classroom, session_date, session_time, subject, week_id) VALUES
    (1, 'Room A101', '2025-01-08', '09:00:00', 'Math Basics', 1),
    (2, 'Lab B203', '2025-01-15', '14:00:00', 'Programming Fundamentals', 2);


--
-- Data for Name: attendance; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.attendance (attendance_id, status, check_at, note, user_id, class_session_id) VALUES
    (1, TRUE, '2025-01-08 08:55:00', 'On time', 1, 1),
    (2, FALSE, '2025-01-15 14:05:00', 'Late by 5 minutes', 2, 2);


--
-- Name: attendance_attendance_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.attendance_attendance_id_seq', 2, TRUE);


--
-- Name: class_session_class_session_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.class_session_class_session_id_seq', 2, TRUE);


--
-- Name: opening_schedule_opening_schedule_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.opening_schedule_opening_schedule_id_seq', 2, TRUE);


--
-- Name: role_role_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.role_role_id_seq', 2, TRUE);


--
-- Name: schedule_schedule_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.schedule_schedule_id_seq', 2, TRUE);


--
-- Name: statistic_statistic_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.statistic_statistic_id_seq', 2, TRUE);


--
-- Name: user_user_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.user_user_id_seq', 2, TRUE);


--
-- Name: week_week_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.week_week_id_seq', 2, TRUE);


--
-- TOC entry 3804 (class 2606 OID 17694)
-- Name: attendance attendance_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.attendance
    ADD CONSTRAINT attendance_pkey PRIMARY KEY (attendance_id);


--
-- TOC entry 3806 (class 2606 OID 17731)
-- Name: class_session class_session_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.class_session
    ADD CONSTRAINT class_session_pkey PRIMARY KEY (class_session_id);


--
-- TOC entry 3798 (class 2606 OID 17654)
-- Name: opening_schedule opening_schedule_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.opening_schedule
    ADD CONSTRAINT opening_schedule_pkey PRIMARY KEY (opening_schedule_id);


--
-- TOC entry 3796 (class 2606 OID 17650)
-- Name: role role_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.role
    ADD CONSTRAINT role_pkey PRIMARY KEY (role_id);


--
-- TOC entry 3800 (class 2606 OID 17662)
-- Name: schedule schedule_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.schedule
    ADD CONSTRAINT schedule_pkey PRIMARY KEY (schedule_id);


--
-- TOC entry 3794 (class 2606 OID 17642)
-- Name: statistic statistic_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.statistic
    ADD CONSTRAINT statistic_pkey PRIMARY KEY (statistic_id);


--
-- TOC entry 3790 (class 2606 OID 17635)
-- Name: user user_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."user"
    ADD CONSTRAINT user_pkey PRIMARY KEY (user_id);


--
-- TOC entry 3792 (class 2606 OID 17644)
-- Name: user user_user_name_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."user"
    ADD CONSTRAINT user_user_name_key UNIQUE (user_name);


--
-- TOC entry 3808 (class 2606 OID 17739)
-- Name: week week_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.week
    ADD CONSTRAINT week_pkey PRIMARY KEY (week_id);


--
-- TOC entry 3810 (class 2606 OID 17695)
-- Name: attendance fk_attendance_class_session; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.attendance
    ADD CONSTRAINT fk_attendance_class_session FOREIGN KEY (class_session_id) REFERENCES public.class_session(class_session_id) ON UPDATE CASCADE ON DELETE CASCADE;


--
-- TOC entry 3811 (class 2606 OID 17700)
-- Name: attendance fk_attendance_user; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.attendance
    ADD CONSTRAINT fk_attendance_user FOREIGN KEY (user_id) REFERENCES public."user"(user_id) ON UPDATE CASCADE ON DELETE CASCADE;


--
-- TOC entry 3817 (class 2606 OID 17740)
-- Name: class_session fk_class_session_week; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.class_session
    ADD CONSTRAINT fk_class_session_week FOREIGN KEY (week_id) REFERENCES public.week(week_id) ON UPDATE CASCADE ON DELETE CASCADE;


--
-- TOC entry 3812 (class 2606 OID 17718)
-- Name: user fk_user_opening_schedule; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."user"
    ADD CONSTRAINT fk_user_opening_schedule FOREIGN KEY (opening_schedule_id) REFERENCES public.opening_schedule(opening_schedule_id) ON UPDATE CASCADE ON DELETE SET NULL;


--
-- TOC entry 3814 (class 2606 OID 17708)
-- Name: user fk_user_role; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."user"
    ADD CONSTRAINT fk_user_role FOREIGN KEY (role_id) REFERENCES public.role(role_id) ON UPDATE CASCADE ON DELETE RESTRICT;


--
-- TOC entry 3815 (class 2606 OID 17723)
-- Name: user fk_user_schedule_personal; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."user"
    ADD CONSTRAINT fk_user_schedule_personal FOREIGN KEY (schedule_id) REFERENCES public.schedule(schedule_id) ON UPDATE CASCADE ON DELETE SET NULL;


--
-- TOC entry 3816 (class 2606 OID 17713)
-- Name: user fk_user_statistic; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."user"
    ADD CONSTRAINT fk_user_statistic FOREIGN KEY (statistic_id) REFERENCES public.statistic(statistic_id) ON UPDATE CASCADE ON DELETE SET NULL;


--
-- TOC entry 3829 (class 2606 OID 17661)
-- Name: week fk_week_schedule; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.week
    ADD CONSTRAINT fk_week_schedule FOREIGN KEY (schedule_id) REFERENCES public.schedule(schedule_id) ON UPDATE CASCADE ON DELETE RESTRICT;


-- PostgreSQL database dump complete
--

-- Custom INSERT statements to add sample data

-- Data for Name: role; Type: TABLE DATA; Schema: public; Owner: postgres
-- (role_id is SERIAL, so we don't insert it. PostgreSQL will auto-increment)
INSERT INTO public.role (role_name) VALUES
    ('Admin'),
    ('Student');


-- Data for Name: statistic; Type: TABLE DATA; Schema: public; Owner: postgres
-- (statistic_id is SERIAL)
INSERT INTO public.statistic (total_score, rank, avg_score) VALUES
    (950, 1, 9.5),
    (880, 2, 8.8);


-- Data for Name: opening_schedule; Type: TABLE DATA; Schema: public; Owner: postgres
-- (opening_schedule_id is SERIAL)
INSERT INTO public.opening_schedule (date_start, date_end, time_start, time_end) VALUES
    ('2025-01-01', '2025-12-31', '08:00:00', '17:00:00'),
    ('2025-02-01', '2025-11-30', '09:00:00', '18:00:00');


-- Data for Name: user; Type: TABLE DATA; Schema: public; Owner: postgres
-- (user_id is SERIAL. statistic_id, role_id, schedule_id, opening_schedule_id are FKs)
-- Assuming role_id 1 is 'Admin' and 2 is 'Student' (from above)
-- Assuming statistic_id 1 and 2 exist (from above)
-- schedule_id and opening_schedule_id can be NULL as per schema definition
INSERT INTO public."user" (user_name, email, password_hash, statistic_id, role_id, schedule_id, opening_schedule_id) VALUES
    ('admin_user', 'admin@example.com', 'hashedpassword_admin_123', 1, 1, NULL, 1),
    ('student_user', 'student@example.com', 'hashedpassword_student_456', 2, 2, NULL, 2);


-- Data for Name: week; Type: TABLE DATA; Schema: public; Owner: postgres
-- (week_id is SERIAL. schedule_id is FK, so we need to ensure schedule exists first)
-- NOTE: The schema has fk_week_schedule which means week needs a schedule_id.
-- So, schedule_id needs to be provided here, matching an existing schedule.
-- Let's make sure schedule is inserted before week.
-- I'll adjust the order of INSERTs slightly to ensure this.
INSERT INTO public.week (week_name, start_date, end_date) VALUES
    ('Week 1', '2025-01-06', '2025-01-12'),
    ('Week 2', '2025-01-13', '2025-01-19');


-- Data for Name: schedule; Type: TABLE DATA; Schema: public; Owner: postgres
-- (schedule_id is SERIAL, week_id is FK)
-- Need to ensure week exists first before inserting into schedule with a week_id.
-- This requires careful ordering.
-- Let's assume week_id 1 and 2 exist from above.
INSERT INTO public.schedule (schedule_name, week_id) VALUES
    ('Morning Classes', 1),
    ('Afternoon Workshops', 2);


-- Data for Name: class_session; Type: TABLE DATA; Schema: public; Owner: postgres
-- (class_session_id is SERIAL, week_id is FK)
-- Assuming week_id 1 and 2 exist from above
INSERT INTO public.class_session (classroom, session_date, session_time, subject, week_id) VALUES
    ('Room A101', '2025-01-08', '09:00:00', 'Math Basics', 1),
    ('Lab B203', '2025-01-15', '14:00:00', 'Programming Fundamentals', 2);


-- Data for Name: attendance; Type: TABLE DATA; Schema: public; Owner: postgres
-- (attendance_id is SERIAL, user_id and class_session_id are FKs)
-- Assuming user_id 1, 2 and class_session_id 1, 2 exist from above
INSERT INTO public.attendance (status, check_at, note, user_id, class_session_id) VALUES
    (TRUE, '2025-01-08 08:55:00', 'On time', 1, 1),
    (FALSE, '2025-01-15 14:05:00', 'Late by 5 minutes', 2, 2);


-- Update sequences to ensure future auto-incremented IDs start correctly
SELECT pg_catalog.setval('public.attendance_attendance_id_seq', 2, TRUE);
SELECT pg_catalog.setval('public.class_session_class_session_id_seq', 2, TRUE);
SELECT pg_catalog.setval('public.opening_schedule_opening_schedule_id_seq', 2, TRUE);
SELECT pg_catalog.setval('public.role_role_id_seq', 2, TRUE);
SELECT pg_catalog.setval('public.schedule_schedule_id_seq', 2, TRUE);
SELECT pg_catalog.setval('public.statistic_statistic_id_seq', 2, TRUE);
SELECT pg_catalog.setval('public.user_user_id_seq', 2, TRUE);
SELECT pg_catalog.setval('public.week_week_id_seq', 2, TRUE);
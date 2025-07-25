-- Migration script to add missing tables and update existing ones

-- Add missing sequences and columns
ALTER TABLE ONLY public.class ALTER COLUMN class_id SET DEFAULT nextval('public.class_class_id_seq'::regclass);
ALTER TABLE ONLY public.instrument ALTER COLUMN instrument_id SET DEFAULT nextval('public.instrument_instrument_id_seq'::regclass);
ALTER TABLE ONLY public.timeslot ALTER COLUMN timeslot_id SET DEFAULT nextval('public.timeslot_timeslot_id_seq'::regclass);
ALTER TABLE ONLY public.room ALTER COLUMN room_id SET DEFAULT nextval('public.room_room_id_seq'::regclass);
ALTER TABLE ONLY public.day ALTER COLUMN day_id SET DEFAULT nextval('public.day_day_id_seq'::regclass);
ALTER TABLE ONLY public.day_of_week_lookup ALTER COLUMN day_of_week_id SET DEFAULT nextval('public.day_of_week_lookup_day_of_week_id_seq'::regclass);

-- Add primary key constraints for new tables
ALTER TABLE ONLY public.class
    ADD CONSTRAINT class_pkey PRIMARY KEY (class_id);

ALTER TABLE ONLY public.class
    ADD CONSTRAINT class_class_code_key UNIQUE (class_code);

ALTER TABLE ONLY public.user_class
    ADD CONSTRAINT user_class_pkey PRIMARY KEY (user_id, class_id);

ALTER TABLE ONLY public.instrument
    ADD CONSTRAINT instrument_pkey PRIMARY KEY (instrument_id);

ALTER TABLE ONLY public.timeslot
    ADD CONSTRAINT timeslot_pkey PRIMARY KEY (timeslot_id);

ALTER TABLE ONLY public.room
    ADD CONSTRAINT room_pkey PRIMARY KEY (room_id);

ALTER TABLE ONLY public.room
    ADD CONSTRAINT room_room_code_key UNIQUE (room_code);

ALTER TABLE ONLY public.day
    ADD CONSTRAINT day_pkey PRIMARY KEY (day_id);

ALTER TABLE ONLY public.day_of_week_lookup
    ADD CONSTRAINT day_of_week_lookup_pkey PRIMARY KEY (day_of_week_id);

ALTER TABLE ONLY public.opening_schedule_day_of_week
    ADD CONSTRAINT opening_schedule_day_of_week_pkey PRIMARY KEY (opening_schedule_id, day_of_week_id);

-- Add foreign key constraints
ALTER TABLE ONLY public.class
    ADD CONSTRAINT fk_class_instrument FOREIGN KEY (instrument_id) REFERENCES public.instrument(instrument_id) ON UPDATE CASCADE ON DELETE RESTRICT;

ALTER TABLE ONLY public.user_class
    ADD CONSTRAINT fk_user_class_user FOREIGN KEY (user_id) REFERENCES public."user"(user_id) ON UPDATE CASCADE ON DELETE CASCADE;

ALTER TABLE ONLY public.user_class
    ADD CONSTRAINT fk_user_class_class FOREIGN KEY (class_id) REFERENCES public.class(class_id) ON UPDATE CASCADE ON DELETE CASCADE;

ALTER TABLE ONLY public.opening_schedule
    ADD CONSTRAINT fk_opening_schedule_teacher FOREIGN KEY (teacher_user_id) REFERENCES public."user"(user_id) ON UPDATE CASCADE ON DELETE SET NULL;

ALTER TABLE ONLY public.opening_schedule
    ADD CONSTRAINT fk_opening_schedule_instrument FOREIGN KEY (instrument_id) REFERENCES public.instrument(instrument_id) ON UPDATE CASCADE ON DELETE RESTRICT;

ALTER TABLE ONLY public.opening_schedule_day_of_week
    ADD CONSTRAINT fk_opening_schedule_day_of_week_opening_schedule FOREIGN KEY (opening_schedule_id) REFERENCES public.opening_schedule(opening_schedule_id) ON UPDATE CASCADE ON DELETE CASCADE;

ALTER TABLE ONLY public.opening_schedule_day_of_week
    ADD CONSTRAINT fk_opening_schedule_day_of_week_day_of_week FOREIGN KEY (day_of_week_id) REFERENCES public.day_of_week_lookup(day_of_week_id) ON UPDATE CASCADE ON DELETE CASCADE;

ALTER TABLE ONLY public.class_session
    ADD CONSTRAINT fk_class_session_day FOREIGN KEY (day_id) REFERENCES public.day(day_id) ON UPDATE CASCADE ON DELETE CASCADE;

ALTER TABLE ONLY public.class_session
    ADD CONSTRAINT fk_class_session_class FOREIGN KEY (class_id) REFERENCES public.class(class_id) ON UPDATE CASCADE ON DELETE CASCADE;

ALTER TABLE ONLY public.class_session
    ADD CONSTRAINT fk_class_session_timeslot FOREIGN KEY (timeslot_id) REFERENCES public.timeslot(timeslot_id) ON UPDATE CASCADE ON DELETE RESTRICT;

ALTER TABLE ONLY public.class_session
    ADD CONSTRAINT fk_class_session_room FOREIGN KEY (room_id) REFERENCES public.room(room_id) ON UPDATE CASCADE ON DELETE RESTRICT;

ALTER TABLE ONLY public.day
    ADD CONSTRAINT fk_day_week FOREIGN KEY (week_id) REFERENCES public.week(week_id) ON UPDATE CASCADE ON DELETE CASCADE;

-- Insert sample data
INSERT INTO public.instrument (instrument_name) VALUES
('Piano'),
('Guitar'),
('Violin'),
('Drums'),
('Flute')
ON CONFLICT DO NOTHING;

INSERT INTO public.timeslot (start_time, end_time, timeslot_name) VALUES
('08:00:00', '09:30:00', 'Morning Session 1'),
('09:45:00', '11:15:00', 'Morning Session 2'),
('14:00:00', '15:30:00', 'Afternoon Session 1'),
('15:45:00', '17:15:00', 'Afternoon Session 2'),
('19:00:00', '20:30:00', 'Evening Session')
ON CONFLICT DO NOTHING;

INSERT INTO public.room (room_code, room_name, capacity) VALUES
('R001', 'Piano Room 1', 10),
('R002', 'Guitar Room 1', 15),
('R003', 'Multi-purpose Room', 20),
('R004', 'Practice Room 1', 5),
('R005', 'Practice Room 2', 5)
ON CONFLICT DO NOTHING;

INSERT INTO public.day_of_week_lookup (day_name, day_number) VALUES
('Monday', 1),
('Tuesday', 2),
('Wednesday', 3),
('Thursday', 4),
('Friday', 5),
('Saturday', 6),
('Sunday', 7)
ON CONFLICT DO NOTHING;

-- Update role table to include Teacher role
INSERT INTO public.role (role_name) VALUES
('Teacher')
ON CONFLICT DO NOTHING;
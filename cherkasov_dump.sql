--
-- PostgreSQL database dump
--

-- Dumped from database version 16.3
-- Dumped by pg_dump version 16.3

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

DROP DATABASE IF EXISTS cherkasov;
--
-- Name: cherkasov; Type: DATABASE; Schema: -; Owner: app
--

CREATE DATABASE cherkasov WITH TEMPLATE = template0 ENCODING = 'UTF8' LOCALE_PROVIDER = libc LOCALE = 'ru_RU.UTF-8';


ALTER DATABASE cherkasov OWNER TO app;

\connect cherkasov

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

--
-- Name: app; Type: SCHEMA; Schema: -; Owner: app
--

CREATE SCHEMA app;


ALTER SCHEMA app OWNER TO app;

--
-- Name: calculate_partner_discount(integer); Type: FUNCTION; Schema: app; Owner: app
--

CREATE FUNCTION app.calculate_partner_discount(p_partner_id integer) RETURNS integer
    LANGUAGE plpgsql
    AS $$
DECLARE
    total_quantity INTEGER;
    new_discount INTEGER;
BEGIN
    -- Считаем общее количество продаж для партнера
    SELECT COALESCE(SUM(quantity), 0) INTO total_quantity
    FROM sales_cherkasov
    WHERE partner_id = p_partner_id;
    
    -- Определяем скидку по шкале
    IF total_quantity < 10000 THEN
        new_discount := 0;
    ELSIF total_quantity < 50000 THEN
        new_discount := 5;
    ELSIF total_quantity < 300000 THEN
        new_discount := 10;
    ELSE
        new_discount := 15;
    END IF;
    
    RETURN new_discount;
END;
$$;


ALTER FUNCTION app.calculate_partner_discount(p_partner_id integer) OWNER TO app;

--
-- Name: calculate_sale_total_cherkasov(); Type: FUNCTION; Schema: app; Owner: app
--

CREATE FUNCTION app.calculate_sale_total_cherkasov() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
DECLARE
    product_price DECIMAL(10, 2);
BEGIN
    SELECT price INTO product_price FROM products_cherkasov WHERE id = NEW.product_id;
    NEW.total_amount = NEW.quantity * product_price;
    RETURN NEW;
END;
$$;


ALTER FUNCTION app.calculate_sale_total_cherkasov() OWNER TO app;

--
-- Name: trigger_update_partner_discount(); Type: FUNCTION; Schema: app; Owner: app
--

CREATE FUNCTION app.trigger_update_partner_discount() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
DECLARE
    affected_partner_id INTEGER;
BEGIN
    -- Определяем ID партнера
    IF TG_OP = 'DELETE' THEN
        affected_partner_id := OLD.partner_id;
    ELSE
        affected_partner_id := NEW.partner_id;
    END IF;
    
    -- Обновляем скидку партнера
    PERFORM update_partner_discount(affected_partner_id);
    
    RETURN NULL;
END;
$$;


ALTER FUNCTION app.trigger_update_partner_discount() OWNER TO app;

--
-- Name: update_partner_discount(integer); Type: FUNCTION; Schema: app; Owner: app
--

CREATE FUNCTION app.update_partner_discount(p_partner_id integer) RETURNS void
    LANGUAGE plpgsql
    AS $$
BEGIN
    UPDATE partners_cherkasov
    SET discount = calculate_partner_discount(p_partner_id)
    WHERE id = p_partner_id;
END;
$$;


ALTER FUNCTION app.update_partner_discount(p_partner_id integer) OWNER TO app;

--
-- Name: update_updated_at_column_cherkasov(); Type: FUNCTION; Schema: app; Owner: app
--

CREATE FUNCTION app.update_updated_at_column_cherkasov() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
BEGIN
    NEW.updated_at = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$;


ALTER FUNCTION app.update_updated_at_column_cherkasov() OWNER TO app;

SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- Name: partner_types_cherkasov; Type: TABLE; Schema: app; Owner: app
--

CREATE TABLE app.partner_types_cherkasov (
    id integer NOT NULL,
    name character varying(100) NOT NULL
);


ALTER TABLE app.partner_types_cherkasov OWNER TO app;

--
-- Name: partner_types_cherkasov_id_seq; Type: SEQUENCE; Schema: app; Owner: app
--

CREATE SEQUENCE app.partner_types_cherkasov_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE app.partner_types_cherkasov_id_seq OWNER TO app;

--
-- Name: partner_types_cherkasov_id_seq; Type: SEQUENCE OWNED BY; Schema: app; Owner: app
--

ALTER SEQUENCE app.partner_types_cherkasov_id_seq OWNED BY app.partner_types_cherkasov.id;


--
-- Name: partners_cherkasov; Type: TABLE; Schema: app; Owner: app
--

CREATE TABLE app.partners_cherkasov (
    id integer NOT NULL,
    type_id integer NOT NULL,
    name character varying(200) NOT NULL,
    director_fullname character varying(200),
    phone character varying(20),
    email character varying(100),
    rating integer,
    created_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    updated_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    discount integer DEFAULT 0,
    address text,
    CONSTRAINT partners_cherkasov_rating_check CHECK ((rating >= 0))
);


ALTER TABLE app.partners_cherkasov OWNER TO app;

--
-- Name: COLUMN partners_cherkasov.discount; Type: COMMENT; Schema: app; Owner: app
--

COMMENT ON COLUMN app.partners_cherkasov.discount IS 'Скидка партнера (в процентах)';


--
-- Name: partners_cherkasov_id_seq; Type: SEQUENCE; Schema: app; Owner: app
--

CREATE SEQUENCE app.partners_cherkasov_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE app.partners_cherkasov_id_seq OWNER TO app;

--
-- Name: partners_cherkasov_id_seq; Type: SEQUENCE OWNED BY; Schema: app; Owner: app
--

ALTER SEQUENCE app.partners_cherkasov_id_seq OWNED BY app.partners_cherkasov.id;


--
-- Name: products_cherkasov; Type: TABLE; Schema: app; Owner: app
--

CREATE TABLE app.products_cherkasov (
    id integer NOT NULL,
    name character varying(200) NOT NULL,
    article character varying(50),
    price numeric(10,2),
    CONSTRAINT products_cherkasov_price_check CHECK ((price > (0)::numeric))
);


ALTER TABLE app.products_cherkasov OWNER TO app;

--
-- Name: products_cherkasov_id_seq; Type: SEQUENCE; Schema: app; Owner: app
--

CREATE SEQUENCE app.products_cherkasov_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE app.products_cherkasov_id_seq OWNER TO app;

--
-- Name: products_cherkasov_id_seq; Type: SEQUENCE OWNED BY; Schema: app; Owner: app
--

ALTER SEQUENCE app.products_cherkasov_id_seq OWNED BY app.products_cherkasov.id;


--
-- Name: sales_cherkasov; Type: TABLE; Schema: app; Owner: app
--

CREATE TABLE app.sales_cherkasov (
    id integer NOT NULL,
    partner_id integer NOT NULL,
    product_id integer NOT NULL,
    quantity integer NOT NULL,
    sale_date date DEFAULT CURRENT_DATE NOT NULL,
    total_amount numeric(10,2),
    created_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT sales_cherkasov_quantity_check CHECK ((quantity > 0))
);


ALTER TABLE app.sales_cherkasov OWNER TO app;

--
-- Name: sales_cherkasov_id_seq; Type: SEQUENCE; Schema: app; Owner: app
--

CREATE SEQUENCE app.sales_cherkasov_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE app.sales_cherkasov_id_seq OWNER TO app;

--
-- Name: sales_cherkasov_id_seq; Type: SEQUENCE OWNED BY; Schema: app; Owner: app
--

ALTER SEQUENCE app.sales_cherkasov_id_seq OWNED BY app.sales_cherkasov.id;


--
-- Name: partner_types_cherkasov id; Type: DEFAULT; Schema: app; Owner: app
--

ALTER TABLE ONLY app.partner_types_cherkasov ALTER COLUMN id SET DEFAULT nextval('app.partner_types_cherkasov_id_seq'::regclass);


--
-- Name: partners_cherkasov id; Type: DEFAULT; Schema: app; Owner: app
--

ALTER TABLE ONLY app.partners_cherkasov ALTER COLUMN id SET DEFAULT nextval('app.partners_cherkasov_id_seq'::regclass);


--
-- Name: products_cherkasov id; Type: DEFAULT; Schema: app; Owner: app
--

ALTER TABLE ONLY app.products_cherkasov ALTER COLUMN id SET DEFAULT nextval('app.products_cherkasov_id_seq'::regclass);


--
-- Name: sales_cherkasov id; Type: DEFAULT; Schema: app; Owner: app
--

ALTER TABLE ONLY app.sales_cherkasov ALTER COLUMN id SET DEFAULT nextval('app.sales_cherkasov_id_seq'::regclass);


--
-- Data for Name: partner_types_cherkasov; Type: TABLE DATA; Schema: app; Owner: app
--

COPY app.partner_types_cherkasov (id, name) FROM stdin;
1	ООО
2	ЗАО
3	ИП
4	АО
5	ПАО
\.


--
-- Data for Name: partners_cherkasov; Type: TABLE DATA; Schema: app; Owner: app
--

COPY app.partners_cherkasov (id, type_id, name, director_fullname, phone, email, rating, created_at, updated_at, discount, address) FROM stdin;
1	1	Газпром	Иванов Иван Иванович	+7(999)123-45-67	russia@gasprom.ru	5	2026-03-14 03:21:08.075792	2026-03-15 22:57:03.475036	5	Санкт-Петербург, Зенит Арена
2	1	Парма	Петров Петр Петрович	+7(999)765-43-21	betcity@parma.ru	5	2026-03-14 03:21:08.075792	2026-03-15 22:59:42.860045	5	Пермь, УДс Молот
3	3	Глина	Сидоров Сидор Сидорович	+7(999)555-55-55	glinko@mail.ru	2	2026-03-14 03:21:08.075792	2026-03-15 23:00:33.338537	0	Москва, Химки
4	1	РЦКР	Меменов Юрий Александрович	+7(999)888-77-66	rckr.com	5	2026-03-13 22:23:18.053463	2026-03-15 23:01:02.968072	0	Пермь, Пушкарская 26
\.


--
-- Data for Name: products_cherkasov; Type: TABLE DATA; Schema: app; Owner: app
--

COPY app.products_cherkasov (id, name, article, price) FROM stdin;
1	Продукт 1	ART001	1000.00
2	Продукт 2	ART002	2500.00
3	Продукт 3	ART003	500.00
4	Продукт 4	ART004	3000.00
5	Продукт 5	ART005	1500.00
\.


--
-- Data for Name: sales_cherkasov; Type: TABLE DATA; Schema: app; Owner: app
--

COPY app.sales_cherkasov (id, partner_id, product_id, quantity, sale_date, total_amount, created_at) FROM stdin;
1	1	1	5000	2024-01-15	5000000.00	2026-03-14 03:21:08.097088
2	1	2	3000	2024-02-20	7500000.00	2026-03-14 03:21:08.097088
3	1	3	8000	2024-03-10	4000000.00	2026-03-14 03:21:08.097088
4	2	4	20000	2024-01-25	60000000.00	2026-03-14 03:21:08.097088
5	2	5	15000	2024-02-28	22500000.00	2026-03-14 03:21:08.097088
6	3	1	1000	2024-03-05	1000000.00	2026-03-14 03:21:08.097088
7	3	3	2000	2024-03-15	1000000.00	2026-03-14 03:21:08.097088
9	4	2	2000	2026-03-02	5000000.00	2026-03-14 04:12:04.398218
\.


--
-- Name: partner_types_cherkasov_id_seq; Type: SEQUENCE SET; Schema: app; Owner: app
--

SELECT pg_catalog.setval('app.partner_types_cherkasov_id_seq', 5, true);


--
-- Name: partners_cherkasov_id_seq; Type: SEQUENCE SET; Schema: app; Owner: app
--

SELECT pg_catalog.setval('app.partners_cherkasov_id_seq', 23, true);


--
-- Name: products_cherkasov_id_seq; Type: SEQUENCE SET; Schema: app; Owner: app
--

SELECT pg_catalog.setval('app.products_cherkasov_id_seq', 5, true);


--
-- Name: sales_cherkasov_id_seq; Type: SEQUENCE SET; Schema: app; Owner: app
--

SELECT pg_catalog.setval('app.sales_cherkasov_id_seq', 9, true);


--
-- Name: partner_types_cherkasov partner_types_cherkasov_name_key; Type: CONSTRAINT; Schema: app; Owner: app
--

ALTER TABLE ONLY app.partner_types_cherkasov
    ADD CONSTRAINT partner_types_cherkasov_name_key UNIQUE (name);


--
-- Name: partner_types_cherkasov partner_types_cherkasov_pkey; Type: CONSTRAINT; Schema: app; Owner: app
--

ALTER TABLE ONLY app.partner_types_cherkasov
    ADD CONSTRAINT partner_types_cherkasov_pkey PRIMARY KEY (id);


--
-- Name: partners_cherkasov partners_cherkasov_pkey; Type: CONSTRAINT; Schema: app; Owner: app
--

ALTER TABLE ONLY app.partners_cherkasov
    ADD CONSTRAINT partners_cherkasov_pkey PRIMARY KEY (id);


--
-- Name: products_cherkasov products_cherkasov_article_key; Type: CONSTRAINT; Schema: app; Owner: app
--

ALTER TABLE ONLY app.products_cherkasov
    ADD CONSTRAINT products_cherkasov_article_key UNIQUE (article);


--
-- Name: products_cherkasov products_cherkasov_pkey; Type: CONSTRAINT; Schema: app; Owner: app
--

ALTER TABLE ONLY app.products_cherkasov
    ADD CONSTRAINT products_cherkasov_pkey PRIMARY KEY (id);


--
-- Name: sales_cherkasov sales_cherkasov_pkey; Type: CONSTRAINT; Schema: app; Owner: app
--

ALTER TABLE ONLY app.sales_cherkasov
    ADD CONSTRAINT sales_cherkasov_pkey PRIMARY KEY (id);


--
-- Name: idx_partners_type_cherkasov; Type: INDEX; Schema: app; Owner: app
--

CREATE INDEX idx_partners_type_cherkasov ON app.partners_cherkasov USING btree (type_id);


--
-- Name: idx_sales_date_cherkasov; Type: INDEX; Schema: app; Owner: app
--

CREATE INDEX idx_sales_date_cherkasov ON app.sales_cherkasov USING btree (sale_date);


--
-- Name: idx_sales_partner_cherkasov; Type: INDEX; Schema: app; Owner: app
--

CREATE INDEX idx_sales_partner_cherkasov ON app.sales_cherkasov USING btree (partner_id);


--
-- Name: idx_sales_product_cherkasov; Type: INDEX; Schema: app; Owner: app
--

CREATE INDEX idx_sales_product_cherkasov ON app.sales_cherkasov USING btree (product_id);


--
-- Name: sales_cherkasov calculate_sale_total_before_insert_cherkasov; Type: TRIGGER; Schema: app; Owner: app
--

CREATE TRIGGER calculate_sale_total_before_insert_cherkasov BEFORE INSERT ON app.sales_cherkasov FOR EACH ROW EXECUTE FUNCTION app.calculate_sale_total_cherkasov();


--
-- Name: sales_cherkasov trigger_update_partner_discount; Type: TRIGGER; Schema: app; Owner: app
--

CREATE TRIGGER trigger_update_partner_discount AFTER INSERT OR DELETE OR UPDATE ON app.sales_cherkasov FOR EACH ROW EXECUTE FUNCTION app.trigger_update_partner_discount();


--
-- Name: partners_cherkasov update_partners_updated_at_cherkasov; Type: TRIGGER; Schema: app; Owner: app
--

CREATE TRIGGER update_partners_updated_at_cherkasov BEFORE UPDATE ON app.partners_cherkasov FOR EACH ROW EXECUTE FUNCTION app.update_updated_at_column_cherkasov();


--
-- Name: partners_cherkasov partners_cherkasov_type_id_fkey; Type: FK CONSTRAINT; Schema: app; Owner: app
--

ALTER TABLE ONLY app.partners_cherkasov
    ADD CONSTRAINT partners_cherkasov_type_id_fkey FOREIGN KEY (type_id) REFERENCES app.partner_types_cherkasov(id) ON DELETE RESTRICT;


--
-- Name: sales_cherkasov sales_cherkasov_partner_id_fkey; Type: FK CONSTRAINT; Schema: app; Owner: app
--

ALTER TABLE ONLY app.sales_cherkasov
    ADD CONSTRAINT sales_cherkasov_partner_id_fkey FOREIGN KEY (partner_id) REFERENCES app.partners_cherkasov(id) ON DELETE CASCADE;


--
-- Name: sales_cherkasov sales_cherkasov_product_id_fkey; Type: FK CONSTRAINT; Schema: app; Owner: app
--

ALTER TABLE ONLY app.sales_cherkasov
    ADD CONSTRAINT sales_cherkasov_product_id_fkey FOREIGN KEY (product_id) REFERENCES app.products_cherkasov(id) ON DELETE RESTRICT;


--
-- PostgreSQL database dump complete
--


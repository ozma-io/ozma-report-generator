--
-- PostgreSQL database dump
--

-- Dumped from database version 13.1
-- Dumped by pg_dump version 13.1

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
-- Name: ReportGenerator; Type: DATABASE; Schema: -; Owner: postgres
--

CREATE DATABASE "ReportGenerator" WITH TEMPLATE = template0 ENCODING = 'UTF8' LOCALE = 'Russian_Russia.1251';


ALTER DATABASE "ReportGenerator" OWNER TO postgres;

\connect "ReportGenerator"

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

SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- Name: ReportTemplateQueries; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."ReportTemplateQueries" (
    "Id" integer NOT NULL,
    "TemplateId" integer NOT NULL,
    "QueryText" character varying NOT NULL,
    "Name" character varying(50) NOT NULL
);


ALTER TABLE public."ReportTemplateQueries" OWNER TO postgres;

--
-- Name: ReportTemplateQueries_Id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public."ReportTemplateQueries_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public."ReportTemplateQueries_Id_seq" OWNER TO postgres;

--
-- Name: ReportTemplateQueries_Id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public."ReportTemplateQueries_Id_seq" OWNED BY public."ReportTemplateQueries"."Id";


--
-- Name: ReportTemplateSchemes; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."ReportTemplateSchemes" (
    "Id" integer NOT NULL,
    "Name" character varying(50) NOT NULL
);


ALTER TABLE public."ReportTemplateSchemes" OWNER TO postgres;

--
-- Name: ReportTemplateSchemes_Id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public."ReportTemplateSchemes_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public."ReportTemplateSchemes_Id_seq" OWNER TO postgres;

--
-- Name: ReportTemplateSchemes_Id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public."ReportTemplateSchemes_Id_seq" OWNED BY public."ReportTemplateSchemes"."Id";


--
-- Name: ReportTemplates; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."ReportTemplates" (
    "Id" integer NOT NULL,
    "SchemeId" integer NOT NULL,
    "Name" character varying(50) NOT NULL,
    "Parameters" character varying,
    "OdtWithoutQueries" bytea NOT NULL
);


ALTER TABLE public."ReportTemplates" OWNER TO postgres;

--
-- Name: ReportTemplates_Id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public."ReportTemplates_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public."ReportTemplates_Id_seq" OWNER TO postgres;

--
-- Name: ReportTemplates_Id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public."ReportTemplates_Id_seq" OWNED BY public."ReportTemplates"."Id";


--
-- Name: vReportTemplates; Type: VIEW; Schema: public; Owner: postgres
--

CREATE VIEW public."vReportTemplates" AS
 SELECT rt."Id",
    rt."Name",
    rt."SchemeId",
    rt."Parameters",
    rts."Name" AS "SchemeName"
   FROM (public."ReportTemplates" rt
     LEFT JOIN public."ReportTemplateSchemes" rts ON ((rts."Id" = rt."SchemeId")));


ALTER TABLE public."vReportTemplates" OWNER TO postgres;

--
-- Name: ReportTemplateQueries Id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."ReportTemplateQueries" ALTER COLUMN "Id" SET DEFAULT nextval('public."ReportTemplateQueries_Id_seq"'::regclass);


--
-- Name: ReportTemplateSchemes Id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."ReportTemplateSchemes" ALTER COLUMN "Id" SET DEFAULT nextval('public."ReportTemplateSchemes_Id_seq"'::regclass);


--
-- Name: ReportTemplates Id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."ReportTemplates" ALTER COLUMN "Id" SET DEFAULT nextval('public."ReportTemplates_Id_seq"'::regclass);


--
-- Data for Name: ReportTemplateQueries; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."ReportTemplateQueries" ("Id", "TemplateId", "QueryText", "Name") FROM stdin;
\.


--
-- Data for Name: ReportTemplateSchemes; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."ReportTemplateSchemes" ("Id", "Name") FROM stdin;
\.


--
-- Data for Name: ReportTemplates; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."ReportTemplates" ("Id", "SchemeId", "Name", "Parameters", "OdtWithoutQueries") FROM stdin;
\.


--
-- Name: ReportTemplateQueries_Id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."ReportTemplateQueries_Id_seq"', 1, false);


--
-- Name: ReportTemplateSchemes_Id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."ReportTemplateSchemes_Id_seq"', 27, true);


--
-- Name: ReportTemplates_Id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."ReportTemplates_Id_seq"', 1, false);


--
-- Name: ReportTemplateQueries reporttemplatequeries_pk; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."ReportTemplateQueries"
    ADD CONSTRAINT reporttemplatequeries_pk PRIMARY KEY ("Id");


--
-- Name: ReportTemplates reporttemplates_pk; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."ReportTemplates"
    ADD CONSTRAINT reporttemplates_pk PRIMARY KEY ("Id");


--
-- Name: ReportTemplateSchemes reporttemplateschemes_pk; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."ReportTemplateSchemes"
    ADD CONSTRAINT reporttemplateschemes_pk PRIMARY KEY ("Id");


--
-- Name: reporttemplatequeries_templateid_index; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX reporttemplatequeries_templateid_index ON public."ReportTemplateQueries" USING btree ("TemplateId");


--
-- Name: reporttemplates_name_uindex; Type: INDEX; Schema: public; Owner: postgres
--

CREATE UNIQUE INDEX reporttemplates_name_uindex ON public."ReportTemplates" USING btree ("Name");


--
-- Name: reporttemplates_schemeid_index; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX reporttemplates_schemeid_index ON public."ReportTemplates" USING btree ("SchemeId");


--
-- Name: reporttemplateschemes_name_uindex; Type: INDEX; Schema: public; Owner: postgres
--

CREATE UNIQUE INDEX reporttemplateschemes_name_uindex ON public."ReportTemplateSchemes" USING btree ("Name");


--
-- Name: ReportTemplateQueries reporttemplatequeries_reporttemplates_id_fk; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."ReportTemplateQueries"
    ADD CONSTRAINT reporttemplatequeries_reporttemplates_id_fk FOREIGN KEY ("TemplateId") REFERENCES public."ReportTemplates"("Id") ON DELETE CASCADE;


--
-- Name: ReportTemplates reporttemplates_reporttemplateschemes_id_fk; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."ReportTemplates"
    ADD CONSTRAINT reporttemplates_reporttemplateschemes_id_fk FOREIGN KEY ("SchemeId") REFERENCES public."ReportTemplateSchemes"("Id") ON DELETE CASCADE;


--
-- PostgreSQL database dump complete
--


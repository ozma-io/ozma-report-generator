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

CREATE DATABASE "ReportGenerator" WITH TEMPLATE = template0 ENCODING = 'UTF8';


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
-- Name: Instances; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."Instances" (
    "Id" integer NOT NULL,
    "Name" character varying(50) NOT NULL
);


ALTER TABLE public."Instances" OWNER TO postgres;

--
-- Name: Instances_Id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public."Instances_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public."Instances_Id_seq" OWNER TO postgres;

--
-- Name: Instances_Id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public."Instances_Id_seq" OWNED BY public."Instances"."Id";


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
-- Name: ReportTemplateSchemas; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."ReportTemplateSchemas" (
    "Id" integer NOT NULL,
    "InstanceId" integer NOT NULL,
    "Name" character varying(50) NOT NULL
);


ALTER TABLE public."ReportTemplateSchemas" OWNER TO postgres;

--
-- Name: ReportTemplateSchemas_Id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public."ReportTemplateSchemas_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public."ReportTemplateSchemas_Id_seq" OWNER TO postgres;

--
-- Name: ReportTemplateSchemas_Id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public."ReportTemplateSchemas_Id_seq" OWNED BY public."ReportTemplateSchemas"."Id";


--
-- Name: ReportTemplates; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."ReportTemplates" (
    "Id" integer NOT NULL,
    "SchemaId" integer NOT NULL,
    "Name" character varying(50) NOT NULL,
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
    rt."SchemaId",
    rts."Name" AS "SchemaName",
    rts."InstanceId"
   FROM (public."ReportTemplates" rt
     LEFT JOIN public."ReportTemplateSchemas" rts ON ((rts."Id" = rt."SchemaId")));


ALTER TABLE public."vReportTemplates" OWNER TO postgres;

--
-- Name: Instances Id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Instances" ALTER COLUMN "Id" SET DEFAULT nextval('public."Instances_Id_seq"'::regclass);


--
-- Name: ReportTemplateQueries Id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."ReportTemplateQueries" ALTER COLUMN "Id" SET DEFAULT nextval('public."ReportTemplateQueries_Id_seq"'::regclass);


--
-- Name: ReportTemplateSchemas Id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."ReportTemplateSchemas" ALTER COLUMN "Id" SET DEFAULT nextval('public."ReportTemplateSchemas_Id_seq"'::regclass);


--
-- Name: ReportTemplates Id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."ReportTemplates" ALTER COLUMN "Id" SET DEFAULT nextval('public."ReportTemplates_Id_seq"'::regclass);


--
-- Data for Name: Instances; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."Instances" ("Id", "Name") FROM stdin;
\.


--
-- Data for Name: ReportTemplateQueries; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."ReportTemplateQueries" ("Id", "TemplateId", "QueryText", "Name") FROM stdin;
\.


--
-- Data for Name: ReportTemplateSchemas; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."ReportTemplateSchemas" ("Id", "InstanceId", "Name") FROM stdin;
\.


--
-- Data for Name: ReportTemplates; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."ReportTemplates" ("Id", "SchemaId", "Name", "OdtWithoutQueries") FROM stdin;
\.


--
-- Name: Instances_Id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."Instances_Id_seq"', 6, true);


--
-- Name: ReportTemplateQueries_Id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."ReportTemplateQueries_Id_seq"', 1, false);


--
-- Name: ReportTemplateSchemas_Id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."ReportTemplateSchemas_Id_seq"', 95, true);


--
-- Name: ReportTemplates_Id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."ReportTemplates_Id_seq"', 1, true);


--
-- Name: Instances instances_pk; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Instances"
    ADD CONSTRAINT instances_pk PRIMARY KEY ("Id");


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
-- Name: ReportTemplateSchemas reporttemplateschemas_pk; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."ReportTemplateSchemas"
    ADD CONSTRAINT reporttemplateschemas_pk PRIMARY KEY ("Id");


--
-- Name: instances_name_uindex; Type: INDEX; Schema: public; Owner: postgres
--

CREATE UNIQUE INDEX instances_name_uindex ON public."Instances" USING btree ("Name");


--
-- Name: reporttemplates_name_schemaid_uindex; Type: INDEX; Schema: public; Owner: postgres
--

CREATE UNIQUE INDEX reporttemplates_name_schemaid_uindex ON public."ReportTemplates" USING btree ("Name", "SchemaId");


--
-- Name: reporttemplates_schemaid_index; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX reporttemplates_schemaid_index ON public."ReportTemplates" USING btree ("SchemaId");


--
-- Name: reporttemplateschemas_instanceid_index; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX reporttemplateschemas_instanceid_index ON public."ReportTemplateSchemas" USING btree ("InstanceId");


--
-- Name: reporttemplateschemas_name_instanceid_uindex; Type: INDEX; Schema: public; Owner: postgres
--

CREATE UNIQUE INDEX reporttemplateschemas_name_instanceid_uindex ON public."ReportTemplateSchemas" USING btree ("Name", "InstanceId");


--
-- Name: ReportTemplateQueries reporttemplatequeries_reporttemplates_id_fk; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."ReportTemplateQueries"
    ADD CONSTRAINT reporttemplatequeries_reporttemplates_id_fk FOREIGN KEY ("TemplateId") REFERENCES public."ReportTemplates"("Id") ON DELETE CASCADE;


--
-- Name: ReportTemplates reporttemplates_reporttemplateschemas_id_fk; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."ReportTemplates"
    ADD CONSTRAINT reporttemplates_reporttemplateschemas_id_fk FOREIGN KEY ("SchemaId") REFERENCES public."ReportTemplateSchemas"("Id") ON DELETE CASCADE;


--
-- Name: ReportTemplateSchemas reporttemplateschemas_instances_id_fk; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."ReportTemplateSchemas"
    ADD CONSTRAINT reporttemplateschemas_instances_id_fk FOREIGN KEY ("InstanceId") REFERENCES public."Instances"("Id") ON DELETE CASCADE;


--
-- PostgreSQL database dump complete
--


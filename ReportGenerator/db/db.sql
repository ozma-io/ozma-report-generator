--
-- PostgreSQL database dump
--

-- Dumped from database version 13.0
-- Dumped by pg_dump version 13.0

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
-- Name: Instances; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."Instances" (
    "Id" integer NOT NULL,
    "Name" character varying(50) NOT NULL
);


--
-- Name: Instances_Id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public."Instances_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: Instances_Id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: -
--

ALTER SEQUENCE public."Instances_Id_seq" OWNED BY public."Instances"."Id";


--
-- Name: ReportTemplateQueries; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."ReportTemplateQueries" (
    "Id" integer NOT NULL,
    "TemplateId" integer NOT NULL,
    "QueryText" character varying NOT NULL,
    "Name" character varying(50) NOT NULL,
    "QueryType" smallint NOT NULL
);


--
-- Name: ReportTemplateQueries_Id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public."ReportTemplateQueries_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: ReportTemplateQueries_Id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: -
--

ALTER SEQUENCE public."ReportTemplateQueries_Id_seq" OWNED BY public."ReportTemplateQueries"."Id";


--
-- Name: ReportTemplateSchemas; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."ReportTemplateSchemas" (
    "Id" integer NOT NULL,
    "InstanceId" integer NOT NULL,
    "Name" character varying(50) NOT NULL
);


--
-- Name: ReportTemplateSchemas_Id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public."ReportTemplateSchemas_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: ReportTemplateSchemas_Id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: -
--

ALTER SEQUENCE public."ReportTemplateSchemas_Id_seq" OWNED BY public."ReportTemplateSchemas"."Id";


--
-- Name: ReportTemplates; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."ReportTemplates" (
    "Id" integer NOT NULL,
    "SchemaId" integer NOT NULL,
    "Name" character varying(50) NOT NULL,
    "OdtWithoutQueries" bytea NOT NULL
);


--
-- Name: ReportTemplates_Id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public."ReportTemplates_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: ReportTemplates_Id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: -
--

ALTER SEQUENCE public."ReportTemplates_Id_seq" OWNED BY public."ReportTemplates"."Id";


--
-- Name: vReportTemplates; Type: VIEW; Schema: public; Owner: -
--

CREATE VIEW public."vReportTemplates" AS
 SELECT rt."Id",
    rt."Name",
    rt."SchemaId",
    rts."Name" AS "SchemaName",
    rts."InstanceId"
   FROM (public."ReportTemplates" rt
     LEFT JOIN public."ReportTemplateSchemas" rts ON ((rts."Id" = rt."SchemaId")));


--
-- Name: Instances Id; Type: DEFAULT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Instances" ALTER COLUMN "Id" SET DEFAULT nextval('public."Instances_Id_seq"'::regclass);


--
-- Name: ReportTemplateQueries Id; Type: DEFAULT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."ReportTemplateQueries" ALTER COLUMN "Id" SET DEFAULT nextval('public."ReportTemplateQueries_Id_seq"'::regclass);


--
-- Name: ReportTemplateSchemas Id; Type: DEFAULT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."ReportTemplateSchemas" ALTER COLUMN "Id" SET DEFAULT nextval('public."ReportTemplateSchemas_Id_seq"'::regclass);


--
-- Name: ReportTemplates Id; Type: DEFAULT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."ReportTemplates" ALTER COLUMN "Id" SET DEFAULT nextval('public."ReportTemplates_Id_seq"'::regclass);


--
-- Name: Instances instances_pk; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Instances"
    ADD CONSTRAINT instances_pk PRIMARY KEY ("Id");


--
-- Name: ReportTemplateQueries reporttemplatequeries_pk; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."ReportTemplateQueries"
    ADD CONSTRAINT reporttemplatequeries_pk PRIMARY KEY ("Id");


--
-- Name: ReportTemplates reporttemplates_pk; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."ReportTemplates"
    ADD CONSTRAINT reporttemplates_pk PRIMARY KEY ("Id");


--
-- Name: ReportTemplateSchemas reporttemplateschemas_pk; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."ReportTemplateSchemas"
    ADD CONSTRAINT reporttemplateschemas_pk PRIMARY KEY ("Id");


--
-- Name: instances_name_uindex; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX instances_name_uindex ON public."Instances" USING btree ("Name");


--
-- Name: reporttemplates_name_schemaid_uindex; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX reporttemplates_name_schemaid_uindex ON public."ReportTemplates" USING btree ("Name", "SchemaId");


--
-- Name: reporttemplates_schemaid_index; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX reporttemplates_schemaid_index ON public."ReportTemplates" USING btree ("SchemaId");


--
-- Name: reporttemplateschemas_instanceid_index; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX reporttemplateschemas_instanceid_index ON public."ReportTemplateSchemas" USING btree ("InstanceId");


--
-- Name: reporttemplateschemas_name_instanceid_uindex; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX reporttemplateschemas_name_instanceid_uindex ON public."ReportTemplateSchemas" USING btree ("Name", "InstanceId");


--
-- Name: ReportTemplateQueries reporttemplatequeries_reporttemplates_id_fk; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."ReportTemplateQueries"
    ADD CONSTRAINT reporttemplatequeries_reporttemplates_id_fk FOREIGN KEY ("TemplateId") REFERENCES public."ReportTemplates"("Id") ON DELETE CASCADE;


--
-- Name: ReportTemplates reporttemplates_reporttemplateschemas_id_fk; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."ReportTemplates"
    ADD CONSTRAINT reporttemplates_reporttemplateschemas_id_fk FOREIGN KEY ("SchemaId") REFERENCES public."ReportTemplateSchemas"("Id") ON DELETE CASCADE;


--
-- Name: ReportTemplateSchemas reporttemplateschemas_instances_id_fk; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."ReportTemplateSchemas"
    ADD CONSTRAINT reporttemplateschemas_instances_id_fk FOREIGN KEY ("InstanceId") REFERENCES public."Instances"("Id") ON DELETE CASCADE;


--
-- PostgreSQL database dump complete
--


--
-- PostgreSQL database dump
--

-- Started on 2008-01-26 15:03:48

SET client_encoding = 'UTF8';
SET standard_conforming_strings = off;
SET check_function_bodies = false;
SET client_min_messages = warning;
SET escape_string_warning = off;

--
-- TOC entry 2107 (class 1262 OID 19507)
-- Name: ftn; Type: DATABASE; Schema: -; Owner: ftn
--

CREATE DATABASE "ftn" WITH TEMPLATE = template0 ENCODING = 'UTF8';


ALTER DATABASE "ftn" OWNER TO ftn;

\connect "ftn"

SET client_encoding = 'UTF8';
SET standard_conforming_strings = off;
SET check_function_bodies = false;
SET client_min_messages = warning;
SET escape_string_warning = off;

--
-- TOC entry 510 (class 2612 OID 16386)
-- Name: plpgsql; Type: PROCEDURAL LANGUAGE; Schema: -; Owner: pgsql
--

CREATE PROCEDURAL LANGUAGE plpgsql;


ALTER PROCEDURAL LANGUAGE plpgsql OWNER TO pgsql;

SET search_path = public, pg_catalog;

--
-- TOC entry 475 (class 1247 OID 19511)
-- Dependencies: 6 1767
-- Name: EchoMessage; Type: TYPE; Schema: public; Owner: ftn
--

CREATE TYPE "EchoMessage" AS (
	mid bigint,
	fromname text,
	fromaddr text,
	toname text,
	toaddr text,
	subject text,
	msgdate text,
	attr integer,
	aka text,
	replayid integer,
	areaname text,
	messagetext text
);


ALTER TYPE public."EchoMessage" OWNER TO ftn;

--
-- TOC entry 477 (class 1247 OID 19514)
-- Dependencies: 6 1768
-- Name: MessageHeader; Type: TYPE; Schema: public; Owner: ftn
--

CREATE TYPE "MessageHeader" AS (
	mid bigint,
	fromname text,
	fromaddr text,
	toname text,
	toaddr text,
	subject text,
	msgdate text,
	attr integer,
	aka text,
	replayid integer
);


ALTER TYPE public."MessageHeader" OWNER TO ftn;

SET default_tablespace = '';

SET default_with_oids = true;

--
-- TOC entry 1769 (class 1259 OID 19524)
-- Dependencies: 2054 6
-- Name: Addrs; Type: TABLE; Schema: public; Owner: ftn; Tablespace: 
--

CREATE TABLE "Addrs" (
    id integer DEFAULT nextval(('public."Addrs_id_seq"'::text)::regclass) NOT NULL,
    "Address" text NOT NULL
);


ALTER TABLE public."Addrs" OWNER TO ftn;

--
-- TOC entry 1771 (class 1259 OID 19533)
-- Dependencies: 2055 2056 2057 2058 6
-- Name: Areas; Type: TABLE; Schema: public; Owner: ftn; Tablespace: 
--

CREATE TABLE "Areas" (
    id integer DEFAULT nextval(('public."Areas_id_seq"'::text)::regclass) NOT NULL,
    "AreaName" text NOT NULL,
    "AKA" text DEFAULT '0:0/0.0'::text NOT NULL,
    "LastreadMsgId" integer DEFAULT 0 NOT NULL,
    "Description" text DEFAULT ''::text NOT NULL
);


ALTER TABLE public."Areas" OWNER TO ftn;

--
-- TOC entry 1772 (class 1259 OID 19543)
-- Dependencies: 2059 6
-- Name: FromUsers; Type: TABLE; Schema: public; Owner: ftn; Tablespace: 
--

CREATE TABLE "FromUsers" (
    id integer DEFAULT nextval(('public."Users_id_seq"'::text)::regclass) NOT NULL,
    "UserName" text NOT NULL
);


ALTER TABLE public."FromUsers" OWNER TO ftn;

--
-- TOC entry 1773 (class 1259 OID 19550)
-- Dependencies: 2060 2061 2062 2063 2064 6
-- Name: MsgBase; Type: TABLE; Schema: public; Owner: ftn; Tablespace: 
--

CREATE TABLE "MsgBase" (
    "Id" integer DEFAULT nextval(('public."MsgBase_Id_seq"'::text)::regclass) NOT NULL,
    "FromNameId" integer NOT NULL,
    "FromAddrId" integer NOT NULL,
    "ToNameId" integer NOT NULL,
    "ToAddr" text,
    "msgDate" text NOT NULL,
    "Attr" integer DEFAULT 0 NOT NULL,
    area_id integer NOT NULL,
    text_id integer NOT NULL,
    "ReplyID" integer DEFAULT 0 NOT NULL,
    msgid text,
    subj_id integer,
    "DateWritten" integer DEFAULT 0 NOT NULL,
    "DateArrived" integer DEFAULT 0 NOT NULL
);


ALTER TABLE public."MsgBase" OWNER TO ftn;

--
-- TOC entry 2110 (class 0 OID 0)
-- Dependencies: 1773
-- Name: COLUMN "MsgBase"."ReplyID"; Type: COMMENT; Schema: public; Owner: ftn
--

COMMENT ON COLUMN "MsgBase"."ReplyID" IS 'Id письма на которое данное является ответом';


--
-- TOC entry 2111 (class 0 OID 0)
-- Dependencies: 1773
-- Name: COLUMN "MsgBase".msgid; Type: COMMENT; Schema: public; Owner: ftn
--

COMMENT ON COLUMN "MsgBase".msgid IS 'MSGID kluge';


SET default_with_oids = false;

--
-- TOC entry 1774 (class 1259 OID 19559)
-- Dependencies: 2065 6
-- Name: MsgTexts; Type: TABLE; Schema: public; Owner: ftn; Tablespace: 
--

CREATE TABLE "MsgTexts" (
    id integer DEFAULT nextval(('public."MsgTexts_id_seq"'::text)::regclass) NOT NULL,
    "MessageText" text
);


ALTER TABLE public."MsgTexts" OWNER TO ftn;

SET default_with_oids = true;

--
-- TOC entry 1784 (class 1259 OID 20491)
-- Dependencies: 6
-- Name: Subjects; Type: TABLE; Schema: public; Owner: ftn; Tablespace: 
--

CREATE TABLE "Subjects" (
    id integer NOT NULL,
    "Subject" text
);


ALTER TABLE public."Subjects" OWNER TO ftn;

--
-- TOC entry 1775 (class 1259 OID 19566)
-- Dependencies: 2066 6
-- Name: ToUsers; Type: TABLE; Schema: public; Owner: ftn; Tablespace: 
--

CREATE TABLE "ToUsers" (
    id integer DEFAULT nextval(('public."ToUsers_id_seq"'::text)::regclass) NOT NULL,
    "UserName" text NOT NULL
);


ALTER TABLE public."ToUsers" OWNER TO ftn;

--
-- TOC entry 1776 (class 1259 OID 19573)
-- Dependencies: 1862 6
-- Name: AllBase; Type: VIEW; Schema: public; Owner: ftn
--

CREATE VIEW "AllBase" AS
    SELECT "FromUsers"."UserName" AS "FromName", "Addrs"."Address" AS "FromAddr", "ToUsers"."UserName" AS "ToName", "MsgBase"."ToAddr", "Subjects"."Subject", "MsgBase"."msgDate", "MsgBase"."Attr", "Areas"."AKA", "MsgBase"."ReplyID", "MsgBase".msgid, "Areas"."AreaName", "MsgTexts"."MessageText" FROM (((((("MsgBase" JOIN "FromUsers" ON (("MsgBase"."FromNameId" = "FromUsers".id))) JOIN "ToUsers" ON (("MsgBase"."ToNameId" = "ToUsers".id))) JOIN "Areas" ON (("MsgBase".area_id = "Areas".id))) JOIN "MsgTexts" ON (("MsgBase".text_id = "MsgTexts".id))) JOIN "Addrs" ON (("MsgBase"."FromAddrId" = "Addrs".id))) JOIN "Subjects" ON (("MsgBase".subj_id = "Subjects".id)));


ALTER TABLE public."AllBase" OWNER TO ftn;

--
-- TOC entry 164 (class 1255 OID 19515)
-- Dependencies: 6 475
-- Name: GetMessage(text, integer); Type: FUNCTION; Schema: public; Owner: ftn
--

CREATE FUNCTION "GetMessage"(text, integer) RETURNS "EchoMessage"
    AS $_$
SELECT setval('msgs_id_seq', 0);
SELECT * FROM (SELECT(nextval('public."msgs_id_seq"'::text)) AS "mid", ("FromUsers"."UserName") AS "FromName", ("Addrs"."Address") AS "FromAddr", ("ToUsers"."UserName") AS "ToName", "MsgBase"."ToAddr", "Subjects"."Subject", "MsgBase"."msgDate", "MsgBase"."Attr", "Areas"."AKA", "MsgBase"."ReplyID", "Areas"."AreaName", "MsgTexts"."MessageText"
   FROM "MsgBase"
   JOIN "FromUsers" ON "MsgBase"."FromNameId" = "FromUsers".id
   JOIN "ToUsers" ON "MsgBase"."ToNameId" = "ToUsers".id
   JOIN "Areas" ON "MsgBase".area_id = "Areas".id
   JOIN "MsgTexts" ON "MsgBase".text_id = "MsgTexts".id
   JOIN "Addrs" ON "MsgBase"."FromAddrId" = "Addrs".id
   JOIN "Subjects" ON "MsgBase".subj_id = "Subjects".id
WHERE ("Areas"."AreaName" = $1) ) AS  msgs
WHERE msgs.mid = $2$_$
    LANGUAGE sql;


ALTER FUNCTION public."GetMessage"(text, integer) OWNER TO ftn;

--
-- TOC entry 2116 (class 0 OID 0)
-- Dependencies: 164
-- Name: FUNCTION "GetMessage"(text, integer); Type: COMMENT; Schema: public; Owner: ftn
--

COMMENT ON FUNCTION "GetMessage"(text, integer) IS '
$1 - имя эхи
$2 - номер мессага';


--
-- TOC entry 162 (class 1255 OID 19516)
-- Dependencies: 510 6
-- Name: GetMessageCount(text); Type: FUNCTION; Schema: public; Owner: ftn
--

CREATE FUNCTION "GetMessageCount"(text) RETURNS integer
    AS $_$DECLARE
   cmsg integer;
BEGIN
SELECT COUNT("MsgBase"."Id") INTO cmsg FROM "MsgBase"
   JOIN "FromUsers" ON "MsgBase"."FromNameId" = "FromUsers".id
   JOIN "ToUsers" ON "MsgBase"."ToNameId" = "ToUsers".id
   JOIN "Areas" ON "MsgBase".area_id = "Areas".id
   JOIN "MsgTexts" ON "MsgBase".text_id = "MsgTexts".id
   JOIN "Addrs" ON "MsgBase"."FromAddrId" = "Addrs".id
   JOIN "Subjects" ON "MsgBase".subj_id = "Subjects".id
WHERE ("Areas"."AreaName" =  $1);
   
  RETURN cmsg;
  
END;$_$
    LANGUAGE plpgsql;


ALTER FUNCTION public."GetMessageCount"(text) OWNER TO ftn;

--
-- TOC entry 2117 (class 0 OID 0)
-- Dependencies: 162
-- Name: FUNCTION "GetMessageCount"(text); Type: COMMENT; Schema: public; Owner: ftn
--

COMMENT ON FUNCTION "GetMessageCount"(text) IS '$1 - имя эхи';


--
-- TOC entry 163 (class 1255 OID 19517)
-- Dependencies: 6 477
-- Name: GetMessageHeaders(text, integer); Type: FUNCTION; Schema: public; Owner: ftn
--

CREATE FUNCTION "GetMessageHeaders"(text, integer) RETURNS "MessageHeader"
    AS $_$
SELECT setval('msgs_id_seq', 0);
SELECT * FROM (SELECT(nextval('public."msgs_id_seq"')) AS "mid", ("FromUsers"."UserName") AS "FromName", ("Addrs"."Address") AS "FromAddr", ("ToUsers"."UserName") AS "ToName", "MsgBase"."ToAddr", "Subjects"."Subject", "MsgBase"."msgDate", "MsgBase"."Attr", "Areas"."AKA", "MsgBase"."ReplyID"
   FROM "MsgBase"
   JOIN "FromUsers" ON "MsgBase"."FromNameId" = "FromUsers".id
   JOIN "ToUsers" ON "MsgBase"."ToNameId" = "ToUsers".id
   JOIN "Areas" ON "MsgBase".area_id = "Areas".id
   JOIN "Addrs" ON "MsgBase"."FromAddrId" = "Addrs".id
   JOIN "Subjects" ON "MsgBase".subj_id = "Subjects".id
WHERE ("Areas"."AreaName" = $1) ) AS  msgs
WHERE msgs.mid = $2$_$
    LANGUAGE sql;


ALTER FUNCTION public."GetMessageHeaders"(text, integer) OWNER TO ftn;

--
-- TOC entry 2118 (class 0 OID 0)
-- Dependencies: 163
-- Name: FUNCTION "GetMessageHeaders"(text, integer); Type: COMMENT; Schema: public; Owner: ftn
--

COMMENT ON FUNCTION "GetMessageHeaders"(text, integer) IS '
$1 - имя эхи
$2 - номер мессага';


--
-- TOC entry 172 (class 1255 OID 19518)
-- Dependencies: 510 6
-- Name: StoreMessage(text, text, text, text, text, text, text, text, text, text, text); Type: FUNCTION; Schema: public; Owner: ftn
--

CREATE FUNCTION "StoreMessage"(text, text, text, text, text, text, text, text, text, text, text) RETURNS integer
    AS $_$DECLARE
	fromNid integer;
	fromAid integer;
	toNid integer;
	areaid integer;
	textid integer;
	subjid integer;
	attr integer;
BEGIN  
	fromNid := get_fromuserid($1);
	fromAid := get_addrsid($2);
	toNid   := get_touserid($3);
	areaid  := get_areasid($5,$10);
	textid  := get_textid($7);
	subjid	:= get_subjectid($6);
        attr	:= CAST($9 AS integer);

	INSERT INTO "MsgBase" ("FromNameId", "FromAddrId", "ToNameId", "ToAddr", "msgDate", "Attr", area_id, text_id, "ReplyID", msgid, subj_id) VALUES 
			      (	fromNid    , fromAid     , toNid     ,   $4    , $8       ,  attr , areaid , textid , 0        , $11, subjid  );

	RETURN 0;

END;
$_$
    LANGUAGE plpgsql;


ALTER FUNCTION public."StoreMessage"(text, text, text, text, text, text, text, text, text, text, text) OWNER TO ftn;

--
-- TOC entry 2119 (class 0 OID 0)
-- Dependencies: 172
-- Name: FUNCTION "StoreMessage"(text, text, text, text, text, text, text, text, text, text, text); Type: COMMENT; Schema: public; Owner: ftn
--

COMMENT ON FUNCTION "StoreMessage"(text, text, text, text, text, text, text, text, text, text, text) IS '
$1 - От кого
$2 - От адрес
$3 - кому
$4 - кому адрес
$5 - в какую эху
$6 - сабж
$7 - текст
$8 - дата
$9 - атрибуты
$10 - источник пакета
$11 - кладж MSGID
';


--
-- TOC entry 165 (class 1255 OID 19519)
-- Dependencies: 510 6
-- Name: get_addrsid(text); Type: FUNCTION; Schema: public; Owner: ftn
--

CREATE FUNCTION get_addrsid(text) RETURNS integer
    AS $_$
DECLARE
	cuid integer;
BEGIN  
	IF length($1) = 0 THEN
	   RETURN 0;
	END IF;
	
	cuid := 0;
	        
	SELECT id INTO cuid FROM "Addrs" WHERE ("Address" = $1);
	
	IF NOT(cuid IS NULL) THEN
	   RETURN cuid;
	END IF;

	--LOCK TABLE "Addrs" IN EXCLUSIVE MODE;
	INSERT INTO "Addrs" ("Address") VALUES ($1);
	SELECT last_value INTO cuid FROM "Addrs_id_seq";
	
	RETURN cuid;
END;
$_$
    LANGUAGE plpgsql;


ALTER FUNCTION public.get_addrsid(text) OWNER TO ftn;

--
-- TOC entry 166 (class 1255 OID 19520)
-- Dependencies: 510 6
-- Name: get_areasid(text, text); Type: FUNCTION; Schema: public; Owner: ftn
--

CREATE FUNCTION get_areasid(text, text) RETURNS integer
    AS $_$
DECLARE
	cuid integer;
BEGIN  
	IF length($1) = 0 THEN
	   RETURN 0;
	END IF;

	IF length($2) = 0 THEN
	   RETURN 0;
	END IF;

	cuid := 0;
	
        SELECT id INTO cuid FROM "Areas" WHERE ("AreaName" = $1) AND ("AKA" = $2);
	
	IF NOT(cuid IS NULL) THEN
	   RETURN cuid;
	END IF;

	--LOCK TABLE "Areas" IN EXCLUSIVE MODE;
	INSERT INTO "Areas" ("AreaName", "AKA") VALUES ($1, $2);
	SELECT last_value INTO cuid FROM "Areas_id_seq";
	
	RETURN cuid;
END;
$_$
    LANGUAGE plpgsql;


ALTER FUNCTION public.get_areasid(text, text) OWNER TO ftn;

--
-- TOC entry 167 (class 1255 OID 19521)
-- Dependencies: 6 510
-- Name: get_fromuserid(text); Type: FUNCTION; Schema: public; Owner: ftn
--

CREATE FUNCTION get_fromuserid(text) RETURNS integer
    AS $_$
DECLARE
	cuid integer;
BEGIN  
	IF length($1) = 0 THEN
	   RETURN 0;
	END IF;

	cuid := 0;
	
        SELECT id INTO cuid FROM "FromUsers" WHERE ("UserName" = $1);
	
	IF NOT(cuid IS NULL) THEN
	   RETURN cuid;
	END IF;

	--LOCK TABLE "FromUsers" IN EXCLUSIVE MODE;
	INSERT INTO "FromUsers" ("UserName") VALUES ($1);
	SELECT last_value INTO cuid FROM "Users_id_seq";
	
	RETURN cuid;
END;
$_$
    LANGUAGE plpgsql;


ALTER FUNCTION public.get_fromuserid(text) OWNER TO ftn;

--
-- TOC entry 170 (class 1255 OID 20509)
-- Dependencies: 6 510
-- Name: get_subjectid(text); Type: FUNCTION; Schema: public; Owner: ftn
--

CREATE FUNCTION get_subjectid(text) RETURNS integer
    AS $_$
DECLARE
	cuid integer;
BEGIN  
	IF length($1) = 0 THEN
	   RETURN 0;
	END IF;
	
	cuid := 0;
	        
	SELECT id INTO cuid FROM "Subjects" WHERE ("Subject" = $1);
	
	IF NOT(cuid IS NULL) THEN
	   RETURN cuid;
	END IF;

	--LOCK TABLE "Subjects" IN EXCLUSIVE MODE;
	INSERT INTO "Subjects" ("Subject") VALUES ($1);
	SELECT last_value INTO cuid FROM "Subjects_id_seq";
	
	RETURN cuid;
END;
$_$
    LANGUAGE plpgsql;


ALTER FUNCTION public.get_subjectid(text) OWNER TO ftn;

--
-- TOC entry 168 (class 1255 OID 19522)
-- Dependencies: 6 510
-- Name: get_textid(text); Type: FUNCTION; Schema: public; Owner: ftn
--

CREATE FUNCTION get_textid(text) RETURNS integer
    AS $_$
DECLARE
	cuid integer;
BEGIN  
	cuid := 0;
	  
        SELECT id INTO cuid FROM "MsgTexts" WHERE ("MessageText" = $1);
	
	IF NOT(cuid IS NULL) THEN
	   RETURN cuid;
	END IF;

	--LOCK TABLE "MsgTexts" IN EXCLUSIVE MODE;
	INSERT INTO "MsgTexts" ("MessageText") VALUES ($1);
	SELECT last_value INTO cuid FROM "MsgTexts_id_seq";
	
	RETURN cuid;
END;
$_$
    LANGUAGE plpgsql;


ALTER FUNCTION public.get_textid(text) OWNER TO ftn;

--
-- TOC entry 169 (class 1255 OID 19523)
-- Dependencies: 510 6
-- Name: get_touserid(text); Type: FUNCTION; Schema: public; Owner: ftn
--

CREATE FUNCTION get_touserid(text) RETURNS integer
    AS $_$
DECLARE
	cuid integer;
BEGIN  
	IF length($1) = 0 THEN
	   RETURN 0;
	END IF;

	cuid := 0;
	
        SELECT id INTO cuid FROM "ToUsers" WHERE ("UserName" = $1);
	
	IF NOT(cuid IS NULL) THEN
	   RETURN cuid;
	END IF;

	--LOCK TABLE "ToUsers" IN EXCLUSIVE MODE;
	INSERT INTO "ToUsers" ("UserName") VALUES ($1);
	SELECT last_value INTO cuid FROM "ToUsers_id_seq";
	
	RETURN cuid;
END;
$_$
    LANGUAGE plpgsql;


ALTER FUNCTION public.get_touserid(text) OWNER TO ftn;

--
-- TOC entry 1770 (class 1259 OID 19531)
-- Dependencies: 6
-- Name: Addrs_id_seq; Type: SEQUENCE; Schema: public; Owner: ftn
--

CREATE SEQUENCE "Addrs_id_seq"
    INCREMENT BY 1
    NO MAXVALUE
    NO MINVALUE
    CACHE 1;


ALTER TABLE public."Addrs_id_seq" OWNER TO ftn;

--
-- TOC entry 1777 (class 1259 OID 19578)
-- Dependencies: 6
-- Name: Areas_id_seq; Type: SEQUENCE; Schema: public; Owner: ftn
--

CREATE SEQUENCE "Areas_id_seq"
    INCREMENT BY 1
    NO MAXVALUE
    NO MINVALUE
    CACHE 1;


ALTER TABLE public."Areas_id_seq" OWNER TO ftn;

--
-- TOC entry 1778 (class 1259 OID 19580)
-- Dependencies: 6
-- Name: MsgBase_Id_seq; Type: SEQUENCE; Schema: public; Owner: ftn
--

CREATE SEQUENCE "MsgBase_Id_seq"
    INCREMENT BY 1
    NO MAXVALUE
    NO MINVALUE
    CACHE 1;


ALTER TABLE public."MsgBase_Id_seq" OWNER TO ftn;

--
-- TOC entry 1779 (class 1259 OID 19582)
-- Dependencies: 6
-- Name: MsgTexts_id_seq; Type: SEQUENCE; Schema: public; Owner: ftn
--

CREATE SEQUENCE "MsgTexts_id_seq"
    INCREMENT BY 1
    NO MAXVALUE
    NO MINVALUE
    CACHE 1;


ALTER TABLE public."MsgTexts_id_seq" OWNER TO ftn;

--
-- TOC entry 1783 (class 1259 OID 20489)
-- Dependencies: 6 1784
-- Name: Subjects_id_seq; Type: SEQUENCE; Schema: public; Owner: ftn
--

CREATE SEQUENCE "Subjects_id_seq"
    INCREMENT BY 1
    NO MAXVALUE
    NO MINVALUE
    CACHE 1;


ALTER TABLE public."Subjects_id_seq" OWNER TO ftn;

--
-- TOC entry 2127 (class 0 OID 0)
-- Dependencies: 1783
-- Name: Subjects_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: ftn
--

ALTER SEQUENCE "Subjects_id_seq" OWNED BY "Subjects".id;


--
-- TOC entry 1780 (class 1259 OID 19584)
-- Dependencies: 6
-- Name: ToUsers_id_seq; Type: SEQUENCE; Schema: public; Owner: ftn
--

CREATE SEQUENCE "ToUsers_id_seq"
    INCREMENT BY 1
    NO MAXVALUE
    NO MINVALUE
    CACHE 1;


ALTER TABLE public."ToUsers_id_seq" OWNER TO ftn;

--
-- TOC entry 1781 (class 1259 OID 19586)
-- Dependencies: 6
-- Name: Users_id_seq; Type: SEQUENCE; Schema: public; Owner: ftn
--

CREATE SEQUENCE "Users_id_seq"
    INCREMENT BY 1
    NO MAXVALUE
    NO MINVALUE
    CACHE 1;


ALTER TABLE public."Users_id_seq" OWNER TO ftn;

--
-- TOC entry 1782 (class 1259 OID 19588)
-- Dependencies: 6
-- Name: msgs_id_seq; Type: SEQUENCE; Schema: public; Owner: ftn
--

CREATE SEQUENCE msgs_id_seq
    INCREMENT BY 1
    NO MAXVALUE
    MINVALUE 0
    CACHE 1;


ALTER TABLE public.msgs_id_seq OWNER TO ftn;

--
-- TOC entry 2067 (class 2604 OID 20494)
-- Dependencies: 1784 1783 1784
-- Name: id; Type: DEFAULT; Schema: public; Owner: ftn
--

ALTER TABLE "Subjects" ALTER COLUMN id SET DEFAULT nextval('"Subjects_id_seq"'::regclass);


--
-- TOC entry 2070 (class 2606 OID 19659)
-- Dependencies: 1769 1769
-- Name: pk_addrs_id; Type: CONSTRAINT; Schema: public; Owner: ftn; Tablespace: 
--

ALTER TABLE ONLY "Addrs"
    ADD CONSTRAINT pk_addrs_id PRIMARY KEY (id);


--
-- TOC entry 2076 (class 2606 OID 19661)
-- Dependencies: 1771 1771
-- Name: pk_areas_id; Type: CONSTRAINT; Schema: public; Owner: ftn; Tablespace: 
--

ALTER TABLE ONLY "Areas"
    ADD CONSTRAINT pk_areas_id PRIMARY KEY (id);


--
-- TOC entry 2098 (class 2606 OID 20496)
-- Dependencies: 1784 1784
-- Name: pk_id; Type: CONSTRAINT; Schema: public; Owner: ftn; Tablespace: 
--

ALTER TABLE ONLY "Subjects"
    ADD CONSTRAINT pk_id PRIMARY KEY (id);


--
-- TOC entry 2089 (class 2606 OID 19663)
-- Dependencies: 1773 1773
-- Name: pk_msgbase_id; Type: CONSTRAINT; Schema: public; Owner: ftn; Tablespace: 
--

ALTER TABLE ONLY "MsgBase"
    ADD CONSTRAINT pk_msgbase_id PRIMARY KEY ("Id");


--
-- TOC entry 2091 (class 2606 OID 19665)
-- Dependencies: 1774 1774
-- Name: pk_msgtext_id; Type: CONSTRAINT; Schema: public; Owner: ftn; Tablespace: 
--

ALTER TABLE ONLY "MsgTexts"
    ADD CONSTRAINT pk_msgtext_id PRIMARY KEY (id);


--
-- TOC entry 2080 (class 2606 OID 19667)
-- Dependencies: 1772 1772
-- Name: pk_users_id; Type: CONSTRAINT; Schema: public; Owner: ftn; Tablespace: 
--

ALTER TABLE ONLY "FromUsers"
    ADD CONSTRAINT pk_users_id PRIMARY KEY (id);


--
-- TOC entry 2095 (class 2606 OID 19669)
-- Dependencies: 1775 1775
-- Name: pk_usersto_id; Type: CONSTRAINT; Schema: public; Owner: ftn; Tablespace: 
--

ALTER TABLE ONLY "ToUsers"
    ADD CONSTRAINT pk_usersto_id PRIMARY KEY (id);


--
-- TOC entry 2081 (class 1259 OID 19670)
-- Dependencies: 1773
-- Name: fki_areas_id; Type: INDEX; Schema: public; Owner: ftn; Tablespace: 
--

CREATE INDEX fki_areas_id ON "MsgBase" USING btree (area_id);


--
-- TOC entry 2082 (class 1259 OID 19671)
-- Dependencies: 1773
-- Name: fki_fromaddr_id; Type: INDEX; Schema: public; Owner: ftn; Tablespace: 
--

CREATE INDEX fki_fromaddr_id ON "MsgBase" USING btree ("FromAddrId");


--
-- TOC entry 2083 (class 1259 OID 19672)
-- Dependencies: 1773
-- Name: fki_fromname_id; Type: INDEX; Schema: public; Owner: ftn; Tablespace: 
--

CREATE INDEX fki_fromname_id ON "MsgBase" USING btree ("FromNameId");


--
-- TOC entry 2084 (class 1259 OID 20644)
-- Dependencies: 1773
-- Name: fki_subjects_id; Type: INDEX; Schema: public; Owner: ftn; Tablespace: 
--

CREATE INDEX fki_subjects_id ON "MsgBase" USING btree (subj_id);


--
-- TOC entry 2085 (class 1259 OID 19673)
-- Dependencies: 1773
-- Name: fki_text_id; Type: INDEX; Schema: public; Owner: ftn; Tablespace: 
--

CREATE INDEX fki_text_id ON "MsgBase" USING btree (text_id);


--
-- TOC entry 2086 (class 1259 OID 19674)
-- Dependencies: 1773
-- Name: fki_toname_id; Type: INDEX; Schema: public; Owner: ftn; Tablespace: 
--

CREATE INDEX fki_toname_id ON "MsgBase" USING btree ("ToNameId");


--
-- TOC entry 2071 (class 1259 OID 20853)
-- Dependencies: 1771
-- Name: id_id; Type: INDEX; Schema: public; Owner: ftn; Tablespace: 
--

CREATE INDEX id_id ON "Areas" USING btree (id);


--
-- TOC entry 2068 (class 1259 OID 19675)
-- Dependencies: 1769
-- Name: idx_addr; Type: INDEX; Schema: public; Owner: ftn; Tablespace: 
--

CREATE UNIQUE INDEX idx_addr ON "Addrs" USING btree ("Address");


--
-- TOC entry 2072 (class 1259 OID 19676)
-- Dependencies: 1771
-- Name: idx_aka; Type: INDEX; Schema: public; Owner: ftn; Tablespace: 
--

CREATE INDEX idx_aka ON "Areas" USING btree ("AKA");


--
-- TOC entry 2073 (class 1259 OID 19677)
-- Dependencies: 1771
-- Name: idx_area; Type: INDEX; Schema: public; Owner: ftn; Tablespace: 
--

CREATE INDEX idx_area ON "Areas" USING btree ("AreaName");

ALTER TABLE "Areas" CLUSTER ON idx_area;


--
-- TOC entry 2077 (class 1259 OID 19678)
-- Dependencies: 1772
-- Name: idx_from; Type: INDEX; Schema: public; Owner: ftn; Tablespace: 
--

CREATE UNIQUE INDEX idx_from ON "FromUsers" USING btree ("UserName");


--
-- TOC entry 2078 (class 1259 OID 20748)
-- Dependencies: 1772
-- Name: idx_id; Type: INDEX; Schema: public; Owner: ftn; Tablespace: 
--

CREATE INDEX idx_id ON "FromUsers" USING btree (id);


--
-- TOC entry 2074 (class 1259 OID 20638)
-- Dependencies: 1771
-- Name: idx_lastread; Type: INDEX; Schema: public; Owner: ftn; Tablespace: 
--

CREATE INDEX idx_lastread ON "Areas" USING btree ("LastreadMsgId");


--
-- TOC entry 2087 (class 1259 OID 19679)
-- Dependencies: 1773
-- Name: idx_msgid; Type: INDEX; Schema: public; Owner: ftn; Tablespace: 
--

CREATE INDEX idx_msgid ON "MsgBase" USING btree (msgid);

ALTER TABLE "MsgBase" CLUSTER ON idx_msgid;


--
-- TOC entry 2096 (class 1259 OID 20502)
-- Dependencies: 1784
-- Name: idx_subj; Type: INDEX; Schema: public; Owner: ftn; Tablespace: 
--

CREATE INDEX idx_subj ON "Subjects" USING btree ("Subject") WITH (fillfactor=10);


--
-- TOC entry 2092 (class 1259 OID 19681)
-- Dependencies: 1775
-- Name: idx_to; Type: INDEX; Schema: public; Owner: ftn; Tablespace: 
--

CREATE UNIQUE INDEX idx_to ON "ToUsers" USING btree ("UserName");


--
-- TOC entry 2093 (class 1259 OID 20959)
-- Dependencies: 1775
-- Name: idx_tousers_id; Type: INDEX; Schema: public; Owner: ftn; Tablespace: 
--

CREATE INDEX idx_tousers_id ON "ToUsers" USING btree (id);


--
-- TOC entry 2099 (class 2606 OID 19682)
-- Dependencies: 1771 2075 1773
-- Name: fk_areas_id; Type: FK CONSTRAINT; Schema: public; Owner: ftn
--

ALTER TABLE ONLY "MsgBase"
    ADD CONSTRAINT fk_areas_id FOREIGN KEY (area_id) REFERENCES "Areas"(id) ON UPDATE RESTRICT ON DELETE RESTRICT;


--
-- TOC entry 2100 (class 2606 OID 19687)
-- Dependencies: 2069 1769 1773
-- Name: fk_fromaddr_id; Type: FK CONSTRAINT; Schema: public; Owner: ftn
--

ALTER TABLE ONLY "MsgBase"
    ADD CONSTRAINT fk_fromaddr_id FOREIGN KEY ("FromAddrId") REFERENCES "Addrs"(id) ON UPDATE RESTRICT ON DELETE RESTRICT;


--
-- TOC entry 2101 (class 2606 OID 19692)
-- Dependencies: 1773 1772 2079
-- Name: fk_fromname_id; Type: FK CONSTRAINT; Schema: public; Owner: ftn
--

ALTER TABLE ONLY "MsgBase"
    ADD CONSTRAINT fk_fromname_id FOREIGN KEY ("FromNameId") REFERENCES "FromUsers"(id) ON UPDATE RESTRICT ON DELETE RESTRICT;


--
-- TOC entry 2102 (class 2606 OID 19697)
-- Dependencies: 1774 1773 2090
-- Name: fk_text_id; Type: FK CONSTRAINT; Schema: public; Owner: ftn
--

ALTER TABLE ONLY "MsgBase"
    ADD CONSTRAINT fk_text_id FOREIGN KEY (text_id) REFERENCES "MsgTexts"(id) ON UPDATE RESTRICT ON DELETE RESTRICT;


--
-- TOC entry 2103 (class 2606 OID 19702)
-- Dependencies: 1773 1775 2094
-- Name: fk_toname_id; Type: FK CONSTRAINT; Schema: public; Owner: ftn
--

ALTER TABLE ONLY "MsgBase"
    ADD CONSTRAINT fk_toname_id FOREIGN KEY ("ToNameId") REFERENCES "ToUsers"(id) ON UPDATE RESTRICT ON DELETE RESTRICT;


--
-- TOC entry 2104 (class 2606 OID 20639)
-- Dependencies: 2097 1784 1773
-- Name: subjects_id; Type: FK CONSTRAINT; Schema: public; Owner: ftn
--

ALTER TABLE ONLY "MsgBase"
    ADD CONSTRAINT subjects_id FOREIGN KEY (subj_id) REFERENCES "Subjects"(id);


--
-- TOC entry 2109 (class 0 OID 0)
-- Dependencies: 6
-- Name: public; Type: ACL; Schema: -; Owner: pgsql
--

REVOKE ALL ON SCHEMA public FROM PUBLIC;
REVOKE ALL ON SCHEMA public FROM pgsql;
GRANT ALL ON SCHEMA public TO pgsql;
GRANT ALL ON SCHEMA public TO PUBLIC;

-- Completed on 2008-01-26 15:03:49

--
-- PostgreSQL database dump complete
--


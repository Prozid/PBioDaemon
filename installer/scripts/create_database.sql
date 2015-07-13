CREATE DATABASE  IF NOT EXISTS `PBioDaemon` /*!40100 DEFAULT CHARACTER SET latin1 */;
USE `PBioDaemon`;
-- MySQL dump 10.13  Distrib 5.5.43, for debian-linux-gnu (i686)
--
-- Host: localhost    Database: PBioDaemon
-- ------------------------------------------------------
-- Server version	5.5.43-0ubuntu0.12.04.1

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `Estado`
--

DROP TABLE IF EXISTS `Estado`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `Estado` (
  `IdEstado` varchar(38) NOT NULL,
  `Nombre` varchar(250) NOT NULL,
  `NombreCorto` varchar(5) NOT NULL,
  PRIMARY KEY (`IdEstado`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `Estado`
--

LOCK TABLES `Estado` WRITE;
/*!40000 ALTER TABLE `Estado` DISABLE KEYS */;
INSERT INTO `Estado` VALUES ('2240fbd0-bf78-11e4-b52e-90f65231cc42','Wait','Wait'),('2cb8f4c2-bf78-11e4-b52e-90f65231cc42','ToRun','ToRun'),('41896c31-bf78-11e4-b52e-90f65231cc42','Run','Run'),('6afa8730-bf78-11e4-b52e-90f65231cc42','Terminate','Term'),('753893a4-bf78-11e4-b52e-90f65231cc42','Error','Error');
/*!40000 ALTER TABLE `Estado` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `Resultado`
--

DROP TABLE IF EXISTS `Resultado`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `Resultado` (
  `IdResultado` varchar(38) NOT NULL DEFAULT '',
  `Xml` text NOT NULL,
  `IdSimulacion` varchar(38) NOT NULL,
  `Proceso_IdProceso` varchar(38) NOT NULL,
  PRIMARY KEY (`IdResultado`),
  KEY `fk_Resultado_Proceso_idx` (`Proceso_IdProceso`),
  CONSTRAINT `fk_Resultado_Proceso` FOREIGN KEY (`Proceso_IdProceso`) REFERENCES `Proceso` (`IdProceso`) ON DELETE NO ACTION ON UPDATE NO ACTION
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `Resultado`
--

LOCK TABLES `Resultado` WRITE;
/*!40000 ALTER TABLE `Resultado` DISABLE KEYS */;
/*!40000 ALTER TABLE `Resultado` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `Proceso`
--

DROP TABLE IF EXISTS `Proceso`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `Proceso` (
  `IdProceso` varchar(38) NOT NULL DEFAULT '',
  `Pid` int(11) DEFAULT NULL,
  `Xml` text NOT NULL,
  `Datos` mediumtext,
  `Estado_IdEstado` varchar(38) NOT NULL,
  PRIMARY KEY (`IdProceso`),
  KEY `fk_Proceso_Estado1_idx` (`Estado_IdEstado`),
  CONSTRAINT `fk_Proceso_Estado1` FOREIGN KEY (`Estado_IdEstado`) REFERENCES `Estado` (`IdEstado`) ON DELETE NO ACTION ON UPDATE NO ACTION
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `Proceso`
--

LOCK TABLES `Proceso` WRITE;
/*!40000 ALTER TABLE `Proceso` DISABLE KEYS */;
/*!40000 ALTER TABLE `Proceso` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `Log`
--

DROP TABLE IF EXISTS `Log`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `Log` (
  `IdLog` varchar(38) NOT NULL DEFAULT '',
  `FechaSimulacion` datetime NOT NULL,
  `Texto` varchar(250) NOT NULL,
  `Proceso_IdProceso` varchar(38) NOT NULL,
  PRIMARY KEY (`IdLog`),
  KEY `fk_Log_Proceso1_idx` (`Proceso_IdProceso`),
  CONSTRAINT `fk_Log_Proceso1` FOREIGN KEY (`Proceso_IdProceso`) REFERENCES `Proceso` (`IdProceso`) ON DELETE NO ACTION ON UPDATE NO ACTION
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `Log`
--

LOCK TABLES `Log` WRITE;
/*!40000 ALTER TABLE `Log` DISABLE KEYS */;
/*!40000 ALTER TABLE `Log` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2015-07-12 14:18:47

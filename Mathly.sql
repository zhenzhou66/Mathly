-- MySQL dump 10.13  Distrib 8.0.46, for Win64 (x86_64)
--
-- Host: 127.0.0.1    Database: test
-- ------------------------------------------------------
-- Server version	8.0.46

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!50503 SET NAMES utf8 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `admininfo`
--

DROP TABLE IF EXISTS `admininfo`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `admininfo` (
  `userID` varchar(255) NOT NULL,
  `adminName` varchar(255) NOT NULL,
  `phoneNumber` varchar(255) DEFAULT NULL,
  `email` varchar(255) DEFAULT NULL,
  `dateJoined` date NOT NULL,
  PRIMARY KEY (`userID`),
  CONSTRAINT `fk_admin_login` FOREIGN KEY (`userID`) REFERENCES `logincredentials` (`userID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `admininfo`
--

LOCK TABLES `admininfo` WRITE;
/*!40000 ALTER TABLE `admininfo` DISABLE KEYS */;
INSERT INTO `admininfo` VALUES ('admin001','Zara Admin','0123456789','zara@mathly.com','2023-01-10');
/*!40000 ALTER TABLE `admininfo` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `badges`
--

DROP TABLE IF EXISTS `badges`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `badges` (
  `badgeID` varchar(255) NOT NULL,
  `expPoints` int NOT NULL,
  `badgeImage` blob NOT NULL,
  PRIMARY KEY (`badgeID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `badges`
--

LOCK TABLES `badges` WRITE;
/*!40000 ALTER TABLE `badges` DISABLE KEYS */;
INSERT INTO `badges` VALUES ('badge001',100,_binary 'bronze_star.png'),('badge002',300,_binary 'silver_star.png'),('badge003',600,_binary 'gold_star.png'),('badge004',1000,_binary 'diamond_star.png');
/*!40000 ALTER TABLE `badges` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `comment`
--

DROP TABLE IF EXISTS `comment`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `comment` (
  `commentID` varchar(255) NOT NULL,
  `userID` varchar(255) NOT NULL,
  `discussionID` varchar(255) NOT NULL,
  `commentText` varchar(255) DEFAULT NULL,
  `likeCount` int NOT NULL,
  PRIMARY KEY (`commentID`),
  KEY `fk_comment_user` (`userID`),
  KEY `fk_comment_discussion` (`discussionID`),
  CONSTRAINT `fk_comment_discussion` FOREIGN KEY (`discussionID`) REFERENCES `discussion` (`discussionID`),
  CONSTRAINT `fk_comment_user` FOREIGN KEY (`userID`) REFERENCES `logincredentials` (`userID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `comment`
--

LOCK TABLES `comment` WRITE;
/*!40000 ALTER TABLE `comment` DISABLE KEYS */;
INSERT INTO `comment` VALUES ('com001','teacher001','disc001','Great question! For substitution, isolate one variable first, then substitute into the second equation. For example, if x + y = 5 and x - y = 1, isolate x = 5 - y then substitute.',5),('com002','student002','disc001','I had the same problem! Watching YouTube videos on substitution really helped me.',3),('com003','teacher002','disc002','SOH CAH TOA is your best friend! Sin = Opposite/Hypotenuse, Cos = Adjacent/Hypotenuse, Tan = Opposite/Adjacent.',8),('com004','student003','disc002','Draw the triangle every time — it really helps visualise which side is which.',4),('com005','teacher002','disc003','Try grouping formulas by shape. Circles, triangles, and quadrilaterals each have their own family of formulas.',6),('com006','student001','disc004','Add all the numbers first then divide by how many there are. A calculator makes it fast!',2),('com007','teacher003','disc004','For large datasets, try grouping data into a frequency table first — it simplifies the calculation a lot.',4),('com008','teacher001','disc005','Algebra is used everywhere — from calculating discounts while shopping to programming and engineering!',7);
/*!40000 ALTER TABLE `comment` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `discussion`
--

DROP TABLE IF EXISTS `discussion`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `discussion` (
  `discussionID` varchar(255) NOT NULL,
  `userID` varchar(255) NOT NULL,
  `questionTitle` varchar(255) DEFAULT NULL,
  `questionText` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`discussionID`),
  KEY `fk_discussion_user` (`userID`),
  CONSTRAINT `fk_discussion_user` FOREIGN KEY (`userID`) REFERENCES `logincredentials` (`userID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `discussion`
--

LOCK TABLES `discussion` WRITE;
/*!40000 ALTER TABLE `discussion` DISABLE KEYS */;
INSERT INTO `discussion` VALUES ('disc001','student001','How to solve simultaneous equations?','I am confused about the substitution method. Can someone explain with an example?'),('disc002','student002','Difference between sin, cos, and tan?','When do I use which trigonometric ratio? I keep mixing them up.'),('disc003','student003','Tips for remembering geometry formulas?','There are so many formulas for area and perimeter. Any memory tricks?'),('disc004','student004','What is the easiest way to find the mean?','Is there a shortcut to calculate mean for large data sets?'),('disc005','student005','Why is algebra important?','I want to understand how algebra is used in real life before I study it.');
/*!40000 ALTER TABLE `discussion` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `learningprogress`
--

DROP TABLE IF EXISTS `learningprogress`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `learningprogress` (
  `progressID` varchar(255) NOT NULL,
  `userID` varchar(255) NOT NULL,
  `topicID` varchar(255) NOT NULL,
  `progressPercentage` double DEFAULT NULL,
  PRIMARY KEY (`progressID`),
  KEY `fk_progress_user` (`userID`),
  KEY `fk_progress_topic` (`topicID`),
  CONSTRAINT `fk_progress_topic` FOREIGN KEY (`topicID`) REFERENCES `topic` (`topicID`),
  CONSTRAINT `fk_progress_user` FOREIGN KEY (`userID`) REFERENCES `studentinfo` (`userID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `learningprogress`
--

LOCK TABLES `learningprogress` WRITE;
/*!40000 ALTER TABLE `learningprogress` DISABLE KEYS */;
INSERT INTO `learningprogress` VALUES ('prog001','student001','topic001',65),('prog002','student001','topic005',40),('prog003','student002','topic001',80),('prog004','student002','topic004',55),('prog005','student003','topic002',90),('prog006','student003','topic003',70),('prog007','student004','topic005',30),('prog008','student005','topic001',50),('prog009','student005','topic003',45);
/*!40000 ALTER TABLE `learningprogress` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `logincredentials`
--

DROP TABLE IF EXISTS `logincredentials`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `logincredentials` (
  `userID` varchar(255) NOT NULL,
  `password` varchar(255) NOT NULL,
  `role` varchar(255) NOT NULL,
  PRIMARY KEY (`userID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `logincredentials`
--

LOCK TABLES `logincredentials` WRITE;
/*!40000 ALTER TABLE `logincredentials` DISABLE KEYS */;
INSERT INTO `logincredentials` VALUES ('admin001','hashed_admin001','admin'),('student001','hashed_student001','student'),('student002','hashed_student002','student'),('student003','hashed_student003','student'),('student004','hashed_student004','student'),('student005','hashed_student005','student'),('student006','zhenzhou','student'),('teacher001','hashed_teacher001','teacher'),('teacher002','hashed_teacher002','teacher'),('teacher003','hashed_teacher003','teacher');
/*!40000 ALTER TABLE `logincredentials` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `notification`
--

DROP TABLE IF EXISTS `notification`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `notification` (
  `notificationID` varchar(255) NOT NULL,
  `userID` varchar(255) NOT NULL,
  `message` varchar(255) NOT NULL,
  `type` varchar(255) NOT NULL,
  `isRead` tinyint(1) NOT NULL,
  PRIMARY KEY (`notificationID`),
  KEY `fk_notification_user` (`userID`),
  CONSTRAINT `fk_notification_user` FOREIGN KEY (`userID`) REFERENCES `logincredentials` (`userID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `notification`
--

LOCK TABLES `notification` WRITE;
/*!40000 ALTER TABLE `notification` DISABLE KEYS */;
INSERT INTO `notification` VALUES ('notif001','student001','You earned the Bronze Star badge!','badge',1),('notif002','student001','New study material added for Algebra.','material',0),('notif003','student002','You earned the Gold Star badge!','badge',1),('notif004','student002','Your Algebra progress is at 80%. Keep it up!','progress',0),('notif005','student003','New quiz available: Geometry Basics.','quiz',0),('notif006','student004','Welcome to Mathly! Start your first topic.','system',1),('notif007','student005','You earned the Silver Star badge!','badge',1),('notif008','teacher001','Student Hafizuddin completed Algebra quiz.','activity',0);
/*!40000 ALTER TABLE `notification` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `quizquestions`
--

DROP TABLE IF EXISTS `quizquestions`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `quizquestions` (
  `questionID` varchar(255) NOT NULL,
  `quizID` varchar(255) NOT NULL,
  `questionText` varchar(255) NOT NULL,
  `questionChoiceA` varchar(255) NOT NULL,
  `questionChoiceB` varchar(255) NOT NULL,
  `questionChoiceC` varchar(255) NOT NULL,
  `questionChoiceD` varchar(255) NOT NULL,
  `questionAnswer` char(1) NOT NULL,
  PRIMARY KEY (`questionID`),
  KEY `fk_question_quiz` (`quizID`),
  CONSTRAINT `fk_question_quiz` FOREIGN KEY (`quizID`) REFERENCES `quizzes` (`quizID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `quizquestions`
--

LOCK TABLES `quizquestions` WRITE;
/*!40000 ALTER TABLE `quizquestions` DISABLE KEYS */;
INSERT INTO `quizquestions` VALUES ('q001','quiz001','Solve for x: 2x + 4 = 10','x = 2','x = 3','x = 4','x = 5','B'),('q002','quiz001','Simplify: 3x + 2x','5x','6x','5x²','x','A'),('q003','quiz001','What is the value of x in: x - 7 = 3?','x = 9','x = 10','x = 4','x = 11','B'),('q004','quiz001','Which of the following is a linear equation?','x² = 4','x³ = 8','2x = 6','x² + y = 1','C'),('q005','quiz002','Solve: 3x - 6 = 9','x = 3','x = 4','x = 5','x = 6','C'),('q006','quiz002','What is the gradient of y = 2x + 3?','3','2','1','5','B'),('q007','quiz003','What is the area of a triangle with base 6 and height 4?','10','12','24','14','B'),('q008','quiz003','How many sides does a hexagon have?','5','6','7','8','B'),('q009','quiz003','What is the sum of angles in a triangle?','90°','180°','270°','360°','B'),('q010','quiz004','What is the mean of 4, 8, 6, 10, 2?','5','6','7','8','B'),('q011','quiz004','What is the median of 3, 7, 1, 9, 5?','3','5','7','9','B'),('q012','quiz005','In a right triangle, sin(θ) equals:','Adjacent/Hypotenuse','Opposite/Hypotenuse','Opposite/Adjacent','Hypotenuse/Opposite','B'),('q013','quiz005','What is cos(60°)?','1','√3/2','1/2','√2/2','C');
/*!40000 ALTER TABLE `quizquestions` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `quizresult`
--

DROP TABLE IF EXISTS `quizresult`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `quizresult` (
  `resultID` varchar(255) NOT NULL,
  `quizID` varchar(255) NOT NULL,
  `userID` varchar(255) NOT NULL,
  `attemptID` varchar(255) NOT NULL,
  `totalQuestionsAmount` int NOT NULL,
  `totalCorrectAnswer` int NOT NULL,
  `score` double NOT NULL,
  PRIMARY KEY (`resultID`),
  KEY `fk_result_quiz` (`quizID`),
  KEY `fk_result_student` (`userID`),
  KEY `fk_result_attempt` (`attemptID`),
  CONSTRAINT `fk_result_attempt` FOREIGN KEY (`attemptID`) REFERENCES `quizstudentattempt` (`attemptID`),
  CONSTRAINT `fk_result_quiz` FOREIGN KEY (`quizID`) REFERENCES `quizzes` (`quizID`),
  CONSTRAINT `fk_result_student` FOREIGN KEY (`userID`) REFERENCES `studentinfo` (`userID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `quizresult`
--

LOCK TABLES `quizresult` WRITE;
/*!40000 ALTER TABLE `quizresult` DISABLE KEYS */;
INSERT INTO `quizresult` VALUES ('res001','quiz001','student001','att001',4,4,100),('res002','quiz001','student002','att005',4,3,75),('res003','quiz003','student003','att009',3,3,100),('res004','quiz004','student005','att012',2,2,100);
/*!40000 ALTER TABLE `quizresult` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `quizstudentattempt`
--

DROP TABLE IF EXISTS `quizstudentattempt`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `quizstudentattempt` (
  `attemptID` varchar(255) NOT NULL,
  `quizID` varchar(255) NOT NULL,
  `userID` varchar(255) NOT NULL,
  `questionID` varchar(255) NOT NULL,
  `studentAnswer` varchar(255) NOT NULL,
  `isCorrect` tinyint(1) NOT NULL,
  `attemptDuration` int NOT NULL,
  PRIMARY KEY (`attemptID`),
  KEY `fk_attempt_quiz` (`quizID`),
  KEY `fk_attempt_student` (`userID`),
  KEY `fk_attempt_question` (`questionID`),
  CONSTRAINT `fk_attempt_question` FOREIGN KEY (`questionID`) REFERENCES `quizquestions` (`questionID`),
  CONSTRAINT `fk_attempt_quiz` FOREIGN KEY (`quizID`) REFERENCES `quizzes` (`quizID`),
  CONSTRAINT `fk_attempt_student` FOREIGN KEY (`userID`) REFERENCES `studentinfo` (`userID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `quizstudentattempt`
--

LOCK TABLES `quizstudentattempt` WRITE;
/*!40000 ALTER TABLE `quizstudentattempt` DISABLE KEYS */;
INSERT INTO `quizstudentattempt` VALUES ('att001','quiz001','student001','q001','B',1,45),('att002','quiz001','student001','q002','A',1,30),('att003','quiz001','student001','q003','B',1,50),('att004','quiz001','student001','q004','C',1,60),('att005','quiz001','student002','q001','B',1,35),('att006','quiz001','student002','q002','A',1,25),('att007','quiz001','student002','q003','A',0,55),('att008','quiz001','student002','q004','C',1,40),('att009','quiz003','student003','q007','B',1,30),('att010','quiz003','student003','q008','B',1,20),('att011','quiz003','student003','q009','B',1,25),('att012','quiz004','student005','q010','B',1,40),('att013','quiz004','student005','q011','B',1,35);
/*!40000 ALTER TABLE `quizstudentattempt` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `quizzes`
--

DROP TABLE IF EXISTS `quizzes`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `quizzes` (
  `quizID` varchar(255) NOT NULL,
  `topicID` varchar(255) DEFAULT NULL,
  `quizTitle` varchar(255) NOT NULL,
  PRIMARY KEY (`quizID`),
  KEY `fk_quiz_topic` (`topicID`),
  CONSTRAINT `fk_quiz_topic` FOREIGN KEY (`topicID`) REFERENCES `topic` (`topicID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `quizzes`
--

LOCK TABLES `quizzes` WRITE;
/*!40000 ALTER TABLE `quizzes` DISABLE KEYS */;
INSERT INTO `quizzes` VALUES ('quiz001','topic001','Algebra Basics Quiz'),('quiz002','topic001','Linear Equations Quiz'),('quiz003','topic002','Geometry Fundamentals Quiz'),('quiz004','topic003','Statistics Quiz'),('quiz005','topic004','Trigonometry Quiz');
/*!40000 ALTER TABLE `quizzes` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `studentbadges`
--

DROP TABLE IF EXISTS `studentbadges`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `studentbadges` (
  `studentBadgeID` varchar(255) NOT NULL,
  `userID` varchar(255) NOT NULL,
  `badgeID` varchar(255) NOT NULL,
  `earnedDate` date NOT NULL,
  PRIMARY KEY (`studentBadgeID`),
  KEY `fk_studentbadge_user` (`userID`),
  KEY `fk_studentbadge_badge` (`badgeID`),
  CONSTRAINT `fk_studentbadge_badge` FOREIGN KEY (`badgeID`) REFERENCES `badges` (`badgeID`),
  CONSTRAINT `fk_studentbadge_user` FOREIGN KEY (`userID`) REFERENCES `studentinfo` (`userID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `studentbadges`
--

LOCK TABLES `studentbadges` WRITE;
/*!40000 ALTER TABLE `studentbadges` DISABLE KEYS */;
INSERT INTO `studentbadges` VALUES ('sb001','student001','badge001','2024-01-20'),('sb002','student001','badge002','2024-02-05'),('sb003','student002','badge001','2024-01-18'),('sb004','student002','badge002','2024-02-01'),('sb005','student002','badge003','2024-03-10'),('sb006','student003','badge001','2024-01-15'),('sb007','student003','badge002','2024-01-28'),('sb008','student003','badge003','2024-02-20'),('sb009','student004','badge001','2024-02-10'),('sb010','student005','badge001','2024-01-25'),('sb011','student005','badge002','2024-02-15');
/*!40000 ALTER TABLE `studentbadges` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `studentinfo`
--

DROP TABLE IF EXISTS `studentinfo`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `studentinfo` (
  `userID` varchar(255) NOT NULL,
  `studentName` varchar(255) DEFAULT NULL,
  `studentAge` int DEFAULT NULL,
  `phoneNumber` varchar(255) DEFAULT NULL,
  `email` varchar(255) DEFAULT NULL,
  `school` varchar(255) DEFAULT NULL,
  `birthDate` date DEFAULT NULL,
  `dateJoined` date NOT NULL,
  `studyLevel` varchar(255) NOT NULL,
  `expPoints` int NOT NULL,
  PRIMARY KEY (`userID`),
  CONSTRAINT `fk_student_login` FOREIGN KEY (`userID`) REFERENCES `logincredentials` (`userID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `studentinfo`
--

LOCK TABLES `studentinfo` WRITE;
/*!40000 ALTER TABLE `studentinfo` DISABLE KEYS */;
INSERT INTO `studentinfo` VALUES ('student001','Hafizuddin Razak',15,'0167891234','hafiz@gmail.com','SMK Taman Desa','2009-03-12','2024-01-05','Form 3',320),('student002','Nurul Aina Zahra',16,'0178902345','aina@gmail.com','SMK Bukit Jalil','2008-07-24','2024-01-08','Form 4',580),('student003','Darren Lim Wei Jie',17,'0189013456','darren@gmail.com','SMK Cheras','2007-11-30','2024-01-10','Form 5',750),('student004','Kavitha Suresh',15,'0190124567','kavitha@gmail.com','SMK Kepong','2009-05-18','2024-01-12','Form 3',210),('student005','Muhammad Irfan',16,'0161235678','irfan@gmail.com','SMK Petaling Jaya','2008-09-09','2024-01-15','Form 4',430),('student006','zhenzhou',15,'0167891234','zz@gmail.com','SMK Taman Desa','2009-04-12','2024-01-05','Form 3',320);
/*!40000 ALTER TABLE `studentinfo` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `studenttopic`
--

DROP TABLE IF EXISTS `studenttopic`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `studenttopic` (
  `studentTopicID` varchar(255) NOT NULL,
  `userID` varchar(255) NOT NULL,
  `topicID` varchar(255) NOT NULL,
  `selectedDate` date NOT NULL,
  PRIMARY KEY (`studentTopicID`),
  KEY `fk_studenttopic_user` (`userID`),
  KEY `fk_studenttopic_topic` (`topicID`),
  CONSTRAINT `fk_studenttopic_topic` FOREIGN KEY (`topicID`) REFERENCES `topic` (`topicID`),
  CONSTRAINT `fk_studenttopic_user` FOREIGN KEY (`userID`) REFERENCES `studentinfo` (`userID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `studenttopic`
--

LOCK TABLES `studenttopic` WRITE;
/*!40000 ALTER TABLE `studenttopic` DISABLE KEYS */;
INSERT INTO `studenttopic` VALUES ('st001','student001','topic001','2024-01-10'),('st002','student001','topic005','2024-01-12'),('st003','student002','topic001','2024-01-09'),('st004','student002','topic004','2024-01-15'),('st005','student003','topic002','2024-01-11'),('st006','student003','topic003','2024-01-13'),('st007','student004','topic005','2024-01-14'),('st008','student005','topic001','2024-01-16'),('st009','student005','topic003','2024-01-18');
/*!40000 ALTER TABLE `studenttopic` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `studymaterial`
--

DROP TABLE IF EXISTS `studymaterial`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `studymaterial` (
  `materialID` varchar(255) NOT NULL,
  `topicID` varchar(255) DEFAULT NULL,
  `fileName` longblob NOT NULL,
  PRIMARY KEY (`materialID`),
  KEY `fk_material_topic` (`topicID`),
  CONSTRAINT `fk_material_topic` FOREIGN KEY (`topicID`) REFERENCES `topic` (`topicID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `studymaterial`
--

LOCK TABLES `studymaterial` WRITE;
/*!40000 ALTER TABLE `studymaterial` DISABLE KEYS */;
INSERT INTO `studymaterial` VALUES ('mat001','topic001',_binary 'algebra_basics.pdf'),('mat002','topic001',_binary 'linear_equations.pdf'),('mat003','topic002',_binary 'introduction_to_geometry.pdf'),('mat004','topic003',_binary 'mean_median_mode.pdf'),('mat005','topic004',_binary 'sin_cos_tan.pdf');
/*!40000 ALTER TABLE `studymaterial` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `teacherinfo`
--

DROP TABLE IF EXISTS `teacherinfo`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `teacherinfo` (
  `userID` varchar(255) NOT NULL,
  `teacherName` varchar(255) DEFAULT NULL,
  `phoneNumber` varchar(255) DEFAULT NULL,
  `email` varchar(255) DEFAULT NULL,
  `topicID` varchar(255) DEFAULT NULL,
  `birthDate` date DEFAULT NULL,
  `dateJoined` date NOT NULL,
  `highestQualification` varchar(255) NOT NULL,
  PRIMARY KEY (`userID`),
  KEY `fk_teacher_topic` (`topicID`),
  CONSTRAINT `fk_teacher_login` FOREIGN KEY (`userID`) REFERENCES `logincredentials` (`userID`),
  CONSTRAINT `fk_teacher_topic` FOREIGN KEY (`topicID`) REFERENCES `topic` (`topicID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `teacherinfo`
--

LOCK TABLES `teacherinfo` WRITE;
/*!40000 ALTER TABLE `teacherinfo` DISABLE KEYS */;
INSERT INTO `teacherinfo` VALUES ('teacher001','Mr. Amir Farhan','0112345678','amir@mathly.com','topic001','1985-04-22','2023-02-01','Master of Education'),('teacher002','Ms. Priya Nair','0123456780','priya@mathly.com','topic002','1990-07-15','2023-02-15','Bachelor of Science'),('teacher003','Mr. David Tan','0134567891','david@mathly.com','topic003','1988-11-03','2023-03-01','Master of Science');
/*!40000 ALTER TABLE `teacherinfo` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `topic`
--

DROP TABLE IF EXISTS `topic`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `topic` (
  `topicID` varchar(255) NOT NULL,
  `topicName` varchar(255) NOT NULL,
  `userID` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`topicID`),
  KEY `fk_topic_teacher` (`userID`),
  CONSTRAINT `fk_topic_teacher` FOREIGN KEY (`userID`) REFERENCES `teacherinfo` (`userID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `topic`
--

LOCK TABLES `topic` WRITE;
/*!40000 ALTER TABLE `topic` DISABLE KEYS */;
INSERT INTO `topic` VALUES ('topic001','Algebra','teacher001'),('topic002','Geometry','teacher002'),('topic003','Statistics','teacher003'),('topic004','Trigonometry','teacher001'),('topic005','Number & Operations','teacher002');
/*!40000 ALTER TABLE `topic` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2026-05-18 23:31:33

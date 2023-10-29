-- phpMyAdmin SQL Dump
-- version 5.1.2
-- https://www.phpmyadmin.net/
--
-- Host: localhost:3306
-- Generation Time: Oct 22, 2023 at 03:48 AM
-- Server version: 5.7.24
-- PHP Version: 8.1.0

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
START TRANSACTION;
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

--
-- Database: `music`
--

-- --------------------------------------------------------

--
-- Table structure for table `albums`
--

CREATE TABLE `albums` (
  `id` int(11) NOT NULL,
  `title` varchar(100) NOT NULL,
  `artist` varchar(100) NOT NULL,
  `year` int(11) NOT NULL,
  `image` varchar(300) DEFAULT NULL,
  `description` varchar(500) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

--
-- Dumping data for table `albums`
--

INSERT INTO `albums` (`id`, `title`, `artist`, `year`, `image`, `description`) VALUES
(3, 'Rubber Soul', 'The Beatles', 1965, 'https://m.media-amazon.com/images/I/81EF5zXRFdL._SL1500_.jpg', 'Rubber Soul is the sixth studio albums by the English rock band the Beatles. It was released on 3 December 1965 in the United Kingdom, on EMI\'s Parlophone label, accompanied by the non-albums double A-side single \"Day Tripper\" / \"We Can Work It Out\".'),
(4, 'Please Please Me', 'The Beatles', 1963, 'https://m.media-amazon.com/images/I/61LdKbic+wL.jpg', 'Please Please Me is the debut studio albums by the English rock band the Beatles.'),
(5, 'With the Beatles', 'The Beatles', 1963, 'https://upload.wikimedia.org/wikipedia/en/0/0a/Withthebeatlescover.jpg', 'With the Beatles is the second studio albums by the English rock band the Beatles.'),
(6, 'A Hard Day\'s Night', 'The Beatles', 1964, 'https://upload.wikimedia.org/wikipedia/en/e/e6/HardDayUK.jpg', 'A Hard Day\'s Night is the third studio albums by the English rock band the Beatles, released on 10 July 1964, with side one containing songs from the soundtracks to their film of the same name.'),
(7, 'Help!', 'The Beatles', 1965, 'https://upload.wikimedia.org/wikipedia/en/thumb/e/e7/Help%21_%28The_Beatles_album_-_cover_art%29.jpg/220px-Help%21_%28The_Beatles_album_-_cover_art%29.jpg', 'Help! is the fifth studio albums by English rock band the Beatles and the soundtracks from their film Help!. It was released on 6 August 1965.'),
(8, 'Sgt. Pepper\'s Lonely Hearts Club Band', 'The Beatles', 1967, 'https://upload.wikimedia.org/wikipedia/en/5/50/Sgt._Pepper%27s_Lonely_Hearts_Club_Band.jpg?20210405225837', 'Sgt. Pepper\'s Lonely Hearts Club Band is the eighth studio albums by the English rock band the Beatles. Released on 26 May 1967 in the United Kingdom[nb 1] and 2 June 1967 in the United States, it spent 27 weeks at number one on the UK albumss Chart and 15 weeks at number one in the US.'),
(9, 'Magical Mystery Tour', 'The Beatles', 1967, 'https://upload.wikimedia.org/wikipedia/en/e/e8/MagicalMysteryTourDoubleEPcover.jpg', 'Magical Mystery Tour is an albums by the English rock band the Beatles that was released as a double EP in the United Kingdom and an LP in the United States.'),
(10, 'The Beatles (White albums)', 'The Beatles', 1968, 'https://upload.wikimedia.org/wikipedia/commons/2/20/TheBeatles68LP.jpg', 'The Beatles, also known as \"The White albums\", is the ninth studio albums by the English rock band the Beatles, released on 22 November 1968.'),
(11, 'Yellow Submarine', 'The Beatles', 1969, 'https://upload.wikimedia.org/wikipedia/en/thumb/a/ac/TheBeatles-YellowSubmarinealbumcover.jpg/220px-TheBeatles-YellowSubmarinealbumcover.jpg', 'Yellow Submarine is the tenth studio albums by English rock band the Beatles, released on 13 January 1969 in the United States and on 17 January 1969 in the United Kingdom.'),
(12, 'Abbey Road', 'The Beatles', 1969, 'https://upload.wikimedia.org/wikipedia/en/4/42/Beatles_-_Abbey_Road.jpg', 'Abbey Road is the eleventh studio albums by English rock band the Beatles, released on 26 September 1969 by Apple Records. The recording sessions for the albums were the last in which all four Beatles participated.'),
(13, 'Let It Be', 'The Beatles', 1970, 'https://upload.wikimedia.org/wikipedia/en/2/25/LetItBe.jpg?20210101011032', 'Let It Be is the twelfth and final studio albums by the English rock band the Beatles. It was released on 8 May 1970, almost a month after the group\'s break-up.');

--
-- Indexes for dumped tables
--

--
-- Indexes for table `albums`
--
ALTER TABLE `albums`
  ADD PRIMARY KEY (`id`);

--
-- AUTO_INCREMENT for dumped tables
--

--
-- AUTO_INCREMENT for table `albums`
--
ALTER TABLE `albums`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=14;
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;

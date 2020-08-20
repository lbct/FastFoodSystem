-- --------------------------------------------------------
-- Host:                         127.0.0.1
-- Versión del servidor:         10.4.11-MariaDB - mariadb.org binary distribution
-- SO del servidor:              Win64
-- HeidiSQL Versión:             11.0.0.5919
-- --------------------------------------------------------

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET NAMES utf8 */;
/*!50503 SET NAMES utf8mb4 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;

-- Volcando estructura para tabla fast_food_db_2.billconfig
CREATE TABLE IF NOT EXISTS `billconfig` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `AuthorizationCode` varchar(50) NOT NULL,
  `DosificationCode` varchar(250) NOT NULL,
  `CurrentBillNumber` int(11) NOT NULL,
  `LimitEmissionDate` date NOT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=latin1;

-- Volcando datos para la tabla fast_food_db_2.billconfig: ~0 rows (aproximadamente)
/*!40000 ALTER TABLE `billconfig` DISABLE KEYS */;
INSERT INTO `billconfig` (`Id`, `AuthorizationCode`, `DosificationCode`, `CurrentBillNumber`, `LimitEmissionDate`) VALUES
	(1, '7904006306693', 'zZ7Z]xssKqkEf_6K9uH(EcV+%x+u[Cca9T%+_$kiLjT8(zr3T9b5Fx2xG-D+_EBS', 2692, '2050-12-01');
/*!40000 ALTER TABLE `billconfig` ENABLE KEYS */;

-- Volcando estructura para tabla fast_food_db_2.cashmovement
CREATE TABLE IF NOT EXISTS `cashmovement` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `LoginId` bigint(20) NOT NULL,
  `Value` decimal(18,2) NOT NULL,
  `DateTime` datetime NOT NULL,
  `Description` varchar(250) NOT NULL,
  `Hide` tinyint(1) NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `LoginId` (`LoginId`),
  CONSTRAINT `cashmovement_ibfk_1` FOREIGN KEY (`LoginId`) REFERENCES `login` (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- Volcando datos para la tabla fast_food_db_2.cashmovement: ~0 rows (aproximadamente)
/*!40000 ALTER TABLE `cashmovement` DISABLE KEYS */;
/*!40000 ALTER TABLE `cashmovement` ENABLE KEYS */;

-- Volcando estructura para tabla fast_food_db_2.categorytype
CREATE TABLE IF NOT EXISTS `categorytype` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Description` varchar(250) NOT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=latin1;

-- Volcando datos para la tabla fast_food_db_2.categorytype: ~0 rows (aproximadamente)
/*!40000 ALTER TABLE `categorytype` DISABLE KEYS */;
INSERT INTO `categorytype` (`Id`, `Description`) VALUES
	(1, 'Sin Categoría');
/*!40000 ALTER TABLE `categorytype` ENABLE KEYS */;

-- Volcando estructura para tabla fast_food_db_2.client
CREATE TABLE IF NOT EXISTS `client` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Nit` varchar(25) NOT NULL,
  `Name` varchar(250) NOT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=latin1;

-- Volcando datos para la tabla fast_food_db_2.client: ~0 rows (aproximadamente)
/*!40000 ALTER TABLE `client` DISABLE KEYS */;
INSERT INTO `client` (`Id`, `Nit`, `Name`) VALUES
	(1, '0', 'SIN NOMBRE');
/*!40000 ALTER TABLE `client` ENABLE KEYS */;

-- Volcando estructura para tabla fast_food_db_2.combo
CREATE TABLE IF NOT EXISTS `combo` (
  `Id` int(11) NOT NULL,
  PRIMARY KEY (`Id`),
  CONSTRAINT `combo_ibfk_1` FOREIGN KEY (`Id`) REFERENCES `product` (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- Volcando datos para la tabla fast_food_db_2.combo: ~0 rows (aproximadamente)
/*!40000 ALTER TABLE `combo` DISABLE KEYS */;
/*!40000 ALTER TABLE `combo` ENABLE KEYS */;

-- Volcando estructura para tabla fast_food_db_2.compoundproduct
CREATE TABLE IF NOT EXISTS `compoundproduct` (
  `Id` int(11) NOT NULL,
  PRIMARY KEY (`Id`),
  CONSTRAINT `compoundproduct_ibfk_1` FOREIGN KEY (`Id`) REFERENCES `product` (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- Volcando datos para la tabla fast_food_db_2.compoundproduct: ~0 rows (aproximadamente)
/*!40000 ALTER TABLE `compoundproduct` DISABLE KEYS */;
/*!40000 ALTER TABLE `compoundproduct` ENABLE KEYS */;

-- Volcando estructura para tabla fast_food_db_2.compoundproductcombo
CREATE TABLE IF NOT EXISTS `compoundproductcombo` (
  `CompoundProductId` int(11) NOT NULL,
  `ComboId` int(11) NOT NULL,
  `RequiredUnits` int(11) NOT NULL,
  PRIMARY KEY (`CompoundProductId`,`ComboId`),
  KEY `ComboId` (`ComboId`),
  CONSTRAINT `compoundproductcombo_ibfk_1` FOREIGN KEY (`CompoundProductId`) REFERENCES `compoundproduct` (`Id`),
  CONSTRAINT `compoundproductcombo_ibfk_2` FOREIGN KEY (`ComboId`) REFERENCES `combo` (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- Volcando datos para la tabla fast_food_db_2.compoundproductcombo: ~0 rows (aproximadamente)
/*!40000 ALTER TABLE `compoundproductcombo` DISABLE KEYS */;
/*!40000 ALTER TABLE `compoundproductcombo` ENABLE KEYS */;

-- Volcando estructura para tabla fast_food_db_2.compoundproductfoodinput
CREATE TABLE IF NOT EXISTS `compoundproductfoodinput` (
  `CompoundProductId` int(11) NOT NULL,
  `FoodInputId` int(11) NOT NULL,
  `RequiredUnits` int(11) NOT NULL,
  PRIMARY KEY (`CompoundProductId`,`FoodInputId`),
  KEY `FoodInputId` (`FoodInputId`),
  CONSTRAINT `compoundproductfoodinput_ibfk_1` FOREIGN KEY (`CompoundProductId`) REFERENCES `compoundproduct` (`Id`),
  CONSTRAINT `compoundproductfoodinput_ibfk_2` FOREIGN KEY (`FoodInputId`) REFERENCES `foodinput` (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- Volcando datos para la tabla fast_food_db_2.compoundproductfoodinput: ~0 rows (aproximadamente)
/*!40000 ALTER TABLE `compoundproductfoodinput` DISABLE KEYS */;
/*!40000 ALTER TABLE `compoundproductfoodinput` ENABLE KEYS */;

-- Volcando estructura para tabla fast_food_db_2.foodinput
CREATE TABLE IF NOT EXISTS `foodinput` (
  `Id` int(11) NOT NULL,
  `UnitTypeId` int(11) NOT NULL,
  `UnitValue` int(11) NOT NULL,
  `UnitCost` decimal(18,2) NOT NULL,
  `Units` int(11) NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `UnitTypeId` (`UnitTypeId`),
  CONSTRAINT `foodinput_ibfk_1` FOREIGN KEY (`UnitTypeId`) REFERENCES `unittype` (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- Volcando datos para la tabla fast_food_db_2.foodinput: ~0 rows (aproximadamente)
/*!40000 ALTER TABLE `foodinput` DISABLE KEYS */;
/*!40000 ALTER TABLE `foodinput` ENABLE KEYS */;

-- Volcando estructura para tabla fast_food_db_2.foodinputcombo
CREATE TABLE IF NOT EXISTS `foodinputcombo` (
  `FoodInputId` int(11) NOT NULL,
  `ComboId` int(11) NOT NULL,
  `RequiredUnits` int(11) NOT NULL,
  PRIMARY KEY (`FoodInputId`,`ComboId`),
  KEY `ComboId` (`ComboId`),
  CONSTRAINT `foodinputcombo_ibfk_1` FOREIGN KEY (`FoodInputId`) REFERENCES `foodinput` (`Id`),
  CONSTRAINT `foodinputcombo_ibfk_2` FOREIGN KEY (`ComboId`) REFERENCES `combo` (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- Volcando datos para la tabla fast_food_db_2.foodinputcombo: ~0 rows (aproximadamente)
/*!40000 ALTER TABLE `foodinputcombo` DISABLE KEYS */;
/*!40000 ALTER TABLE `foodinputcombo` ENABLE KEYS */;

-- Volcando estructura para procedimiento fast_food_db_2.GetCashMovement
DELIMITER //
CREATE PROCEDURE `GetCashMovement`(
	p_login_id bigint
)
BEGIN
select * 
from CashMovement as c 
where c.LoginId=p_login_id and c.Hide = 0;
end//
DELIMITER ;

-- Volcando estructura para función fast_food_db_2.GetProductCost
DELIMITER //
CREATE FUNCTION `GetProductCost`(p_ProductId int
) RETURNS decimal(18,2)
BEGIN
	return COALESCE(
	(
		select sp.UnitCost from SimpleProduct as sp
		where sp.Id = p_ProductId
	), 0) +
	COALESCE(
	(
		select fi.UnitCost from FoodInput as fi
		where fi.Id = p_ProductId
	), 0) +
	COALESCE(
	(
		select sum(fi.UnitCost * rel.RequiredUnits) 
		from CompoundProduct as cp,
		CompoundProductFoodInput as rel,
		FoodInput as fi
		where
		cp.Id = rel.CompoundProductId AND
		fi.Id = rel.FoodInputId AND
		cp.Id = p_ProductId
	), 0) +
	COALESCE(
	(
		select sum(fi.UnitCost * relcp.RequiredUnits * relc.RequiredUnits) 
		from Combo as c,
		CompoundProductCombo as relc,
		CompoundProduct as cp,
		CompoundProductFoodInput as relcp,
		FoodInput as fi
		where c.Id = relc.ComboId AND
		relc.CompoundProductId = cp.Id AND
		cp.Id = relcp.CompoundProductId AND
		fi.Id = relcp.FoodInputId AND
		c.Id = p_ProductId
	), 0) + 
	COALESCE(
	(
		select sum(sp.UnitCost * rel.RequiredUnits) 
		from Combo as c,
		SimpleProductCombo as rel,
		SimpleProduct as sp
		where
		c.Id = rel.ComboId AND
		sp.Id = rel.SimpleProductId AND
		c.Id = p_ProductId
	), 0) + 
	COALESCE(
	(
		select sum(fi.UnitCost * rel.RequiredUnits)
		from Combo as c,
		FoodInputCombo as rel,
		FoodInput as fi
		where
		c.Id = rel.ComboId AND
		fi.Id = rel.FoodInputId AND
		c.Id = p_ProductId
	), 0);
END//
DELIMITER ;

-- Volcando estructura para función fast_food_db_2.GetProductType
DELIMITER //
CREATE FUNCTION `GetProductType`(`p_ProductId` int

) RETURNS varchar(250) CHARSET latin1
BEGIN
	return IF((select COALESCE(
	(
		select 1 from Combo as c
		where c.Id = p_ProductId
	), 0)) = 1, 'Combo',
	IF((select COALESCE(
	(
		select 1 from SimpleProduct as sp
		where sp.Id = p_ProductId
	), 0)) = 1, 'Producto Simple', 
	IF((select COALESCE(
	(
		select 1 from CompoundProduct as cp
		where cp.Id = p_ProductId
	), 0)) = 1, 'Producto Compuesto', 'Insumo')));
END//
DELIMITER ;

-- Volcando estructura para función fast_food_db_2.GetProductUnits
DELIMITER //
CREATE FUNCTION `GetProductUnits`(`p_ProductId` int

) RETURNS int(11)
BEGIN
	RETURN COALESCE(
	(
		select sp.Units from SimpleProduct as sp
		where sp.Id = p_ProductId
	), 0) +
	COALESCE(
	(
		select fi.Units from FoodInput as fi
		where fi.Id = p_ProductId
	), 0) + 
	COALESCE(
	(
		select Min(CAST((fi.Units / rel.RequiredUnits) as INT)) 
		from CompoundProduct as cp,
		CompoundProductFoodInput as rel,
		FoodInput as fi
		where cp.Id = rel.CompoundProductId AND
		fi.Id = rel.FoodInputId AND
		cp.Id = p_ProductId
	), 0) +
	COALESCE(
	(
		select
		IF(result.Compound <= result.Simple AND result.Compound <= result.Input AND result.Compound < 1000000000, result.Compound,
		IF(result.Simple <= result.Compound AND result.Simple <= result.Input AND result.Simple < 1000000000, result.Simple, 
		IF(result.Input < 1000000000, result.Input, 0)))
		from (select COALESCE(
			(
				select MIN(CAST((sp.Units / relsp.RequiredUnits)  as INT))
				from Combo as c,
				SimpleProduct as sp,
				SimpleProductCombo as relsp
				where
				c.Id = relsp.ComboId AND
				sp.Id = relsp.SimpleProductId AND
				c.Id = p_ProductId
			), 1000000000) as Simple,
			COALESCE(
			(
				select Min(CAST((fi.Units / rel.RequiredUnits) as INT))
				from Combo as c,
				FoodInputCombo as rel,
				FoodInput as fi
				where
				c.Id = rel.ComboId AND
				fi.Id = rel.FoodInputId AND
				c.Id = p_ProductId
			), 1000000000) as Input,
			COALESCE(
			(
				select MIN(result.value) from (
				select CAST((MIN(CAST((fi.Units / relfi.RequiredUnits) as INT)) / MIN(rel.RequiredUnits)) as INT) as value
				from Combo as c,
				CompoundProductCombo as rel,
				CompoundProduct as cp,
				CompoundProductFoodInput as relfi,
				FoodInput as fi
				where
				c.Id = rel.ComboId AND
				cp.Id = rel.CompoundProductId AND
				cp.Id = relfi.CompoundProductId AND
				fi.Id = relfi.FoodInputId AND
				c.Id = p_ProductId
				group by cp.Id) as result
			), 1000000000) as Compound) as result
	), 0);
END//
DELIMITER ;

-- Volcando estructura para procedimiento fast_food_db_2.GetPurchaseDetail
DELIMITER //
CREATE PROCEDURE `GetPurchaseDetail`(
	p_start_date Datetime(3),
	p_end_date Datetime(3)
)
BEGIN 
select 
	p.Id as PurchaseId,
	pd.Id as PurchaseDetailId,
	pr.Id as ProductId,
	u.FullName as SystemUser,
	p.DateTime,
	pr.Description as ProductDescription,
	pd.Units,
	pd.Units * pd.UnitValue as TotalValue
	from Purchase as p,
	PurchaseDetail as pd,
	Product as pr,
	Login as l,
	User as u
	where 
	p.Id = pd.PurchaseId AND
	pd.ProductId = pr.Id AND
	l.Id = p.LoginId AND
	l.UserId = u.Id AND
	p.Hide = 0
	and p.DateTime between p_start_date and p_end_date;
end//
DELIMITER ;

-- Volcando estructura para procedimiento fast_food_db_2.GetPurchaseDetailByLogin
DELIMITER //
CREATE PROCEDURE `GetPurchaseDetailByLogin`(
	p_login_id bigint
)
BEGIN 
select 
	p.Id as PurchaseId,
	pd.Id as PurchaseDetailId,
	pr.Id as ProductId,
	u.FullName as SystemUser,
	p.DateTime,
	pr.Description as ProductDescription,
	pd.Units,
	pd.Units * pd.UnitValue as TotalValue
	from Purchase as p,
	PurchaseDetail as pd,
	Product as pr,
	Login as l,
	User as u
	where 
	p.Id = pd.PurchaseId AND
	pd.ProductId = pr.Id AND
	l.Id = p.LoginId AND
	l.UserId = u.Id AND
	p.Hide = 0
	and p.LoginId = p_login_id;
end//
DELIMITER ;

-- Volcando estructura para procedimiento fast_food_db_2.GetSaleDetail
DELIMITER //
CREATE PROCEDURE `GetSaleDetail`(p_start_date DateTime, p_end_date DateTime)
BEGIN
select 
	s.Id as SaleId,
	d.Id as SaleDetailId,
	s.BillNumber,
	s.ControlCode,
	d.ProductId,
	u.FullName as SystemUser,
	c.Name as ClientName,
	c.Nit as ClientNit,
	s.DateTime,
	p.Description as ProductDescription,
	d.Units,
	d.UnitValue * d.Units * (1 - (d.DiscountValue / 100)) as TotalValue,
	d.UnitCost * d.Units as TotalCost
	from SaleDetail as d,
	Sale as s,
	Login as l,
	User as u,
	Client as c,
	Product as p
	where 
	s.Id = d.SaleId
	and l.Id = s.LoginId
	and u.Id = l.UserId
	and c.Id = s.ClientId
	and p.Id = d.ProductId
	and s.Hide = 0
	and s.DateTime between p_start_date and p_end_date;
END//
DELIMITER ;

-- Volcando estructura para procedimiento fast_food_db_2.GetSaleDetailByLogin
DELIMITER //
CREATE PROCEDURE `GetSaleDetailByLogin`(
	p_login_id bigint
)
BEGIN 
select 
	s.Id as SaleId,
	d.Id as SaleDetailId,
	d.ProductId,
	u.FullName as SystemUser,
	c.Name as ClientName,
	c.Nit as ClientNit,
	s.DateTime,
	p.Description as ProductDescription,
	d.Units,
	d.UnitValue * d.Units * (1 - (d.DiscountValue / 100)) as TotalValue,
	d.UnitCost * d.Units as TotalCost
	from SaleDetail as d,
	Sale as s,
	Login as l,
	User as u,
	Client as c,
	Product as p
	where 
	s.Id = d.SaleId
	and l.Id = s.LoginId
	and u.Id = l.UserId
	and c.Id = s.ClientId
	and p.Id = d.ProductId
	and s.Hide = 0
	and s.LoginId = p_login_id;
end//
DELIMITER ;

-- Volcando estructura para tabla fast_food_db_2.login
CREATE TABLE IF NOT EXISTS `login` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `UserId` int(11) NOT NULL,
  `StartDateTime` datetime NOT NULL,
  `EndDateTime` datetime DEFAULT NULL,
  `StartCashValue` decimal(18,2) NOT NULL,
  `EndCashValue` decimal(18,2) DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `UserId` (`UserId`),
  CONSTRAINT `login_ibfk_1` FOREIGN KEY (`UserId`) REFERENCES `user` (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- Volcando datos para la tabla fast_food_db_2.login: ~0 rows (aproximadamente)
/*!40000 ALTER TABLE `login` DISABLE KEYS */;
/*!40000 ALTER TABLE `login` ENABLE KEYS */;

-- Volcando estructura para tabla fast_food_db_2.product
CREATE TABLE IF NOT EXISTS `product` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `CategoryTypeId` int(11) NOT NULL,
  `Description` varchar(256) NOT NULL,
  `SaleValue` decimal(18,2) NOT NULL,
  `ImagePath` varchar(512) DEFAULT NULL,
  `SaleDiscount` decimal(18,2) NOT NULL,
  `Hide` tinyint(1) NOT NULL,
  `HideForSales` tinyint(1) NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `CategoryTypeId` (`CategoryTypeId`),
  CONSTRAINT `product_ibfk_1` FOREIGN KEY (`CategoryTypeId`) REFERENCES `categorytype` (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- Volcando datos para la tabla fast_food_db_2.product: ~0 rows (aproximadamente)
/*!40000 ALTER TABLE `product` DISABLE KEYS */;
/*!40000 ALTER TABLE `product` ENABLE KEYS */;

-- Volcando estructura para tabla fast_food_db_2.productproviderrelation
CREATE TABLE IF NOT EXISTS `productproviderrelation` (
  `ProviderId` int(11) NOT NULL,
  `ProductId` int(11) NOT NULL,
  PRIMARY KEY (`ProviderId`,`ProductId`),
  KEY `ProductId` (`ProductId`),
  CONSTRAINT `productproviderrelation_ibfk_1` FOREIGN KEY (`ProviderId`) REFERENCES `provider` (`Id`),
  CONSTRAINT `productproviderrelation_ibfk_2` FOREIGN KEY (`ProductId`) REFERENCES `product` (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- Volcando datos para la tabla fast_food_db_2.productproviderrelation: ~0 rows (aproximadamente)
/*!40000 ALTER TABLE `productproviderrelation` DISABLE KEYS */;
/*!40000 ALTER TABLE `productproviderrelation` ENABLE KEYS */;

-- Volcando estructura para vista fast_food_db_2.productview
-- Creando tabla temporal para superar errores de dependencia de VIEW
CREATE TABLE `productview` (
	`Id` INT(11) NOT NULL,
	`ProductType` VARCHAR(250) NULL COLLATE 'latin1_swedish_ci',
	`CategoryTypeId` INT(11) NOT NULL,
	`CategoryDescription` VARCHAR(250) NULL COLLATE 'latin1_swedish_ci',
	`Description` VARCHAR(256) NOT NULL COLLATE 'latin1_swedish_ci',
	`UnitSaleValue` DECIMAL(18,2) NOT NULL,
	`UnitCost` DECIMAL(18,2) NULL,
	`AvailableUnits` INT(11) NULL,
	`SaleDiscount` DECIMAL(18,2) NOT NULL,
	`ImagePath` VARCHAR(512) NULL COLLATE 'latin1_swedish_ci',
	`HideForSales` TINYINT(1) NOT NULL
) ENGINE=MyISAM;

-- Volcando estructura para tabla fast_food_db_2.provider
CREATE TABLE IF NOT EXISTS `provider` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `FullName` varchar(250) NOT NULL,
  `PhoneNumber` varchar(250) DEFAULT NULL,
  `EMail` varchar(250) DEFAULT NULL,
  `Description` varchar(250) DEFAULT NULL,
  `Hide` tinyint(1) NOT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- Volcando datos para la tabla fast_food_db_2.provider: ~0 rows (aproximadamente)
/*!40000 ALTER TABLE `provider` DISABLE KEYS */;
/*!40000 ALTER TABLE `provider` ENABLE KEYS */;

-- Volcando estructura para tabla fast_food_db_2.purchase
CREATE TABLE IF NOT EXISTS `purchase` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `LoginId` bigint(20) NOT NULL,
  `DateTime` datetime NOT NULL,
  `Hide` tinyint(1) NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `LoginId` (`LoginId`),
  CONSTRAINT `purchase_ibfk_1` FOREIGN KEY (`LoginId`) REFERENCES `login` (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- Volcando datos para la tabla fast_food_db_2.purchase: ~0 rows (aproximadamente)
/*!40000 ALTER TABLE `purchase` DISABLE KEYS */;
/*!40000 ALTER TABLE `purchase` ENABLE KEYS */;

-- Volcando estructura para tabla fast_food_db_2.purchasedetail
CREATE TABLE IF NOT EXISTS `purchasedetail` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `PurchaseId` bigint(20) NOT NULL,
  `ProductId` int(11) NOT NULL,
  `Units` int(11) NOT NULL,
  `UnitValue` decimal(18,2) NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `PurchaseId` (`PurchaseId`),
  KEY `ProductId` (`ProductId`),
  CONSTRAINT `purchasedetail_ibfk_1` FOREIGN KEY (`PurchaseId`) REFERENCES `purchase` (`Id`),
  CONSTRAINT `purchasedetail_ibfk_2` FOREIGN KEY (`ProductId`) REFERENCES `product` (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- Volcando datos para la tabla fast_food_db_2.purchasedetail: ~0 rows (aproximadamente)
/*!40000 ALTER TABLE `purchasedetail` DISABLE KEYS */;
/*!40000 ALTER TABLE `purchasedetail` ENABLE KEYS */;

-- Volcando estructura para tabla fast_food_db_2.sale
CREATE TABLE IF NOT EXISTS `sale` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `LoginId` bigint(20) NOT NULL,
  `SaleTypeId` int(11) NOT NULL,
  `ClientId` int(11) NOT NULL,
  `DailyId` int(11) NOT NULL,
  `BillNumber` int(11) NOT NULL,
  `ControlCode` varchar(50) NOT NULL,
  `DateTime` datetime NOT NULL,
  `Hide` tinyint(1) NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `LoginId` (`LoginId`),
  KEY `SaleTypeId` (`SaleTypeId`),
  KEY `ClientId` (`ClientId`),
  CONSTRAINT `sale_ibfk_1` FOREIGN KEY (`LoginId`) REFERENCES `login` (`Id`),
  CONSTRAINT `sale_ibfk_2` FOREIGN KEY (`SaleTypeId`) REFERENCES `saletype` (`Id`),
  CONSTRAINT `sale_ibfk_3` FOREIGN KEY (`ClientId`) REFERENCES `client` (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- Volcando datos para la tabla fast_food_db_2.sale: ~0 rows (aproximadamente)
/*!40000 ALTER TABLE `sale` DISABLE KEYS */;
/*!40000 ALTER TABLE `sale` ENABLE KEYS */;

-- Volcando estructura para tabla fast_food_db_2.saledetail
CREATE TABLE IF NOT EXISTS `saledetail` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `SaleId` bigint(20) NOT NULL,
  `ProductId` int(11) NOT NULL,
  `Units` int(11) NOT NULL,
  `UnitValue` decimal(18,2) NOT NULL,
  `UnitCost` decimal(18,2) NOT NULL,
  `DiscountValue` decimal(18,2) NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `SaleId` (`SaleId`),
  KEY `ProductId` (`ProductId`),
  CONSTRAINT `saledetail_ibfk_1` FOREIGN KEY (`SaleId`) REFERENCES `sale` (`Id`),
  CONSTRAINT `saledetail_ibfk_2` FOREIGN KEY (`ProductId`) REFERENCES `product` (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- Volcando datos para la tabla fast_food_db_2.saledetail: ~0 rows (aproximadamente)
/*!40000 ALTER TABLE `saledetail` DISABLE KEYS */;
/*!40000 ALTER TABLE `saledetail` ENABLE KEYS */;

-- Volcando estructura para tabla fast_food_db_2.saleorder
CREATE TABLE IF NOT EXISTS `saleorder` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `LoginId` bigint(20) NOT NULL,
  `OrderName` varchar(150) NOT NULL,
  `Observation` varchar(250) NOT NULL,
  `DailyId` int(11) NOT NULL,
  `DateTime` datetime NOT NULL,
  `Hide` tinyint(1) NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `LoginId` (`LoginId`),
  CONSTRAINT `saleorder_ibfk_1` FOREIGN KEY (`LoginId`) REFERENCES `login` (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- Volcando datos para la tabla fast_food_db_2.saleorder: ~0 rows (aproximadamente)
/*!40000 ALTER TABLE `saleorder` DISABLE KEYS */;
/*!40000 ALTER TABLE `saleorder` ENABLE KEYS */;

-- Volcando estructura para tabla fast_food_db_2.saleorderdetail
CREATE TABLE IF NOT EXISTS `saleorderdetail` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `SaleOrderId` bigint(20) NOT NULL,
  `ProductId` int(11) NOT NULL,
  `Units` int(11) NOT NULL,
  `UnitValue` decimal(18,2) NOT NULL,
  `UnitCost` decimal(18,2) NOT NULL,
  `DiscountValue` decimal(18,2) NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `SaleOrderId` (`SaleOrderId`),
  CONSTRAINT `saleorderdetail_ibfk_1` FOREIGN KEY (`SaleOrderId`) REFERENCES `saleorder` (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- Volcando datos para la tabla fast_food_db_2.saleorderdetail: ~0 rows (aproximadamente)
/*!40000 ALTER TABLE `saleorderdetail` DISABLE KEYS */;
/*!40000 ALTER TABLE `saleorderdetail` ENABLE KEYS */;

-- Volcando estructura para tabla fast_food_db_2.saletype
CREATE TABLE IF NOT EXISTS `saletype` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Description` varchar(250) NOT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=latin1;

-- Volcando datos para la tabla fast_food_db_2.saletype: ~0 rows (aproximadamente)
/*!40000 ALTER TABLE `saletype` DISABLE KEYS */;
INSERT INTO `saletype` (`Id`, `Description`) VALUES
	(1, 'Default');
/*!40000 ALTER TABLE `saletype` ENABLE KEYS */;

-- Volcando estructura para tabla fast_food_db_2.simpleproduct
CREATE TABLE IF NOT EXISTS `simpleproduct` (
  `Id` int(11) NOT NULL,
  `Units` int(11) NOT NULL,
  `UnitCost` decimal(18,2) NOT NULL,
  PRIMARY KEY (`Id`),
  CONSTRAINT `simpleproduct_ibfk_1` FOREIGN KEY (`Id`) REFERENCES `product` (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- Volcando datos para la tabla fast_food_db_2.simpleproduct: ~0 rows (aproximadamente)
/*!40000 ALTER TABLE `simpleproduct` DISABLE KEYS */;
/*!40000 ALTER TABLE `simpleproduct` ENABLE KEYS */;

-- Volcando estructura para tabla fast_food_db_2.simpleproductcombo
CREATE TABLE IF NOT EXISTS `simpleproductcombo` (
  `SimpleProductId` int(11) NOT NULL,
  `ComboId` int(11) NOT NULL,
  `RequiredUnits` int(11) NOT NULL,
  PRIMARY KEY (`SimpleProductId`,`ComboId`),
  KEY `ComboId` (`ComboId`),
  CONSTRAINT `simpleproductcombo_ibfk_1` FOREIGN KEY (`SimpleProductId`) REFERENCES `simpleproduct` (`Id`),
  CONSTRAINT `simpleproductcombo_ibfk_2` FOREIGN KEY (`ComboId`) REFERENCES `combo` (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- Volcando datos para la tabla fast_food_db_2.simpleproductcombo: ~0 rows (aproximadamente)
/*!40000 ALTER TABLE `simpleproductcombo` DISABLE KEYS */;
/*!40000 ALTER TABLE `simpleproductcombo` ENABLE KEYS */;

-- Volcando estructura para tabla fast_food_db_2.unittype
CREATE TABLE IF NOT EXISTS `unittype` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `ShortName` varchar(10) NOT NULL,
  `Description` varchar(250) NOT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=9 DEFAULT CHARSET=latin1;

-- Volcando datos para la tabla fast_food_db_2.unittype: ~8 rows (aproximadamente)
/*!40000 ALTER TABLE `unittype` DISABLE KEYS */;
INSERT INTO `unittype` (`Id`, `ShortName`, `Description`) VALUES
	(1, 'g', 'Gramo'),
	(2, 'kg', 'Kilogramo'),
	(3, 'dag', 'Decagramo'),
	(4, 'oz', 'Onza'),
	(5, 'lb', 'Libra'),
	(6, 'l', 'Litro'),
	(7, 'ml', 'Mililitro'),
	(8, 'mg', 'Miligramo');
/*!40000 ALTER TABLE `unittype` ENABLE KEYS */;

-- Volcando estructura para tabla fast_food_db_2.user
CREATE TABLE IF NOT EXISTS `user` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Username` varchar(250) NOT NULL,
  `Password` varchar(250) NOT NULL,
  `FullName` varchar(250) NOT NULL,
  `Admin` tinyint(1) NOT NULL DEFAULT 0,
  `Hide` tinyint(1) NOT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=latin1;

-- Volcando datos para la tabla fast_food_db_2.user: ~0 rows (aproximadamente)
/*!40000 ALTER TABLE `user` DISABLE KEYS */;
INSERT INTO `user` (`Id`, `Username`, `Password`, `FullName`, `Admin`, `Hide`) VALUES
	(1, 'admin', '21232f297a57a5a743894a0e4a801fc3', 'Administrator', 1, 0);
/*!40000 ALTER TABLE `user` ENABLE KEYS */;

-- Volcando estructura para vista fast_food_db_2.productview
-- Eliminando tabla temporal y crear estructura final de VIEW
DROP TABLE IF EXISTS `productview`;
CREATE ALGORITHM=UNDEFINED SQL SECURITY DEFINER VIEW `productview` AS select 
p.Id,
(
	select GetProductType(p.Id)
) as ProductType,
p.CategoryTypeId,
(
	select ct.Description 
	from CategoryType as ct
	where ct.Id = p.CategoryTypeId
) as CategoryDescription,
p.Description,
p.SaleValue as UnitSaleValue,
(
	select GetProductCost(p.Id)
) as UnitCost,
(
	select GetProductUnits(p.Id)
) as AvailableUnits,
p.SaleDiscount,
p.ImagePath,
p.HideForSales
from 
Product as p
where
p.Hide = 0 ;

/*!40101 SET SQL_MODE=IFNULL(@OLD_SQL_MODE, '') */;
/*!40014 SET FOREIGN_KEY_CHECKS=IF(@OLD_FOREIGN_KEY_CHECKS IS NULL, 1, @OLD_FOREIGN_KEY_CHECKS) */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;

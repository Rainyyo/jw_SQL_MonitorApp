/*
 Navicat Premium Data Transfer

 Source Server         : 192.168.200.9
 Source Server Type    : MySQL
 Source Server Version : 50744
 Source Host           : 192.168.200.9:3306
 Source Schema         : jw-eap

 Target Server Type    : MySQL
 Target Server Version : 50744
 File Encoding         : 65001

 Date: 23/03/2024 18:37:52
*/

SET NAMES utf8mb4;
SET FOREIGN_KEY_CHECKS = 0;

-- ----------------------------
-- Table structure for pc_data_alarm
-- ----------------------------
DROP TABLE IF EXISTS `pc_data_alarm`;
CREATE TABLE `pc_data_alarm`  (
  `ID` int(11) NOT NULL AUTO_INCREMENT,
  `ALARMCODE` varchar(30) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL COMMENT '报警代码',
  `ALARMDES` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL COMMENT '报警描述',
  `ALARMVALUE` varchar(1) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL COMMENT '报警值(0/1)：0复位，1报警',
  `CREATETIME` datetime NULL DEFAULT NULL COMMENT '时间:yyyy-mm-dd hh:mm:ss',
  PRIMARY KEY (`ID`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 1 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_general_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Records of pc_data_alarm
-- ----------------------------

-- ----------------------------
-- Table structure for pc_data_craft
-- ----------------------------
DROP TABLE IF EXISTS `pc_data_craft`;
CREATE TABLE `pc_data_craft`  (
  `ID` int(11) NOT NULL AUTO_INCREMENT,
  `CONTAINER` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL COMMENT '批次号',
  `PNLIDORSETID` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL COMMENT '板号\r\n（SETID）',
  `ITEMCODE` varchar(30) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL COMMENT '采集项目代码',
  `ITEMDES` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL COMMENT '采集项目描述',
  `UOM` varchar(30) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL COMMENT '单位',
  `ITEMVALUE` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL COMMENT '采集值',
  `ISOK` varchar(1) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL COMMENT '合格标志',
  `GROUPNUM` BIGINT NULL DEFAULT NULL COMMENT '组号：设备对每一次数据采集数据产生一个唯一组号\r\n时间转换成yyyyMMddhhmmssff',
  `CREATETIME` datetime NULL DEFAULT NULL COMMENT '时间:yyyy-mm-dd hh:mm:ss',
  PRIMARY KEY (`ID`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 1 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_general_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Records of pc_data_craft
-- ----------------------------

-- ----------------------------
-- Table structure for pc_data_emp
-- ----------------------------
DROP TABLE IF EXISTS `pc_data_emp`;
CREATE TABLE `pc_data_emp`  (
  `ID` int(11) NOT NULL AUTO_INCREMENT,
  `EMPNO` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL COMMENT '员工工号\r\n（必填项）',
  `EMPNAME` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL COMMENT '员工姓名\r\n（可为空）',
  `EMPCODE` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL COMMENT '操作代码\r\n(上机代码：INTIME\r\n下机代码：OUTTIME)',
  `EMPDES` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL COMMENT '操作名称',
  `EMPTIME` datetime NULL DEFAULT NULL COMMENT '操作时间\r\nyyyy-mm-dd hh:mm:ss',
  `CREATETIME` datetime NULL DEFAULT NULL COMMENT '时间\r\nyyyy-mm-dd hh:mm:ss',
  PRIMARY KEY (`ID`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 1 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_general_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Records of pc_data_emp
-- ----------------------------

-- ----------------------------
-- Table structure for pc_signal_status
-- ----------------------------
DROP TABLE IF EXISTS `pc_signal_status`;
CREATE TABLE `pc_signal_status`  (
  `ID` int(11) NOT NULL AUTO_INCREMENT,
  `SIGNALCODE` varchar(30) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL COMMENT '信号代码',
  `SIGNALDES` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL COMMENT '信号描述',
  `SIGNALVALUE` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL COMMENT '信号值',
  `CREATETIME` datetime NULL DEFAULT NULL,
  PRIMARY KEY (`ID`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 1 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_general_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Records of pc_signal_status
-- ----------------------------

SET FOREIGN_KEY_CHECKS = 1;

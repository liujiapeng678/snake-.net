﻿import { AcGameObject } from "./AcGameObject";
import { Cell } from "./Cell";

export class Snake extends AcGameObject {
    constructor(info, gamemap) {
        super()

        this.id = info.id
        this.color = info.color
        this.gamemap = gamemap

        this.cells = [new Cell(info.r, info.c)] // 蛇的身体，由一个个Cell组成
        this.speed = 5
        this.direction = -1  // 0 1 2 3上右下左
        this.status = "idle"  // idle, move, die

        this.next_cell = null
        this.dr = [-1, 0, 1, 0]
        this.dc = [0, 1, 0, -1]
        this.step = 0   // 回合数

        this.eps = 1e-2

        this.eye_direction = 0
        if (this.id == 1) this.eye_direction = 2
        this.eye_dx = [
            [-1, 1],
            [1, 1],
            [1, -1],
            [-1, -1]
        ]
        this.eye_dy = [
            [-1, -1],
            [-1, 1],
            [1, 1],
            [1, -1]
        ]
    }

    check_tail_increasing() {  // 检测蛇长度增加
        //if(this.step <= 10)return true
        if (this.step % 3 === 1) return true
        return false
    }

    set_direction(d) {
        this.direction = d
    }

    start() {

    }
    next_step() {   //更新蛇的状态到下一步
        const d = this.direction

        this.next_cell = new Cell(this.cells[0].r + this.dr[d], this.cells[0].c + this.dc[d])
        this.eye_direction = d
        this.direction = -1
        this.status = "move"
        this.step++

        const len = this.cells.length
        for (let i = len; i > 0; i--) {
            this.cells[i] = JSON.parse(JSON.stringify(this.cells[i - 1]))
        }

        // if(!this.gamemap.check_valid(this.next_cell)){   // 目标格子会发生碰撞
        //     this.status = "die"
        //     return
        // }
        //
    }
    update_move() {
        const dx = this.next_cell.x - this.cells[0].x, dy = this.next_cell.y - this.cells[0].y
        const distance = Math.sqrt(dx * dx + dy * dy)
        if (distance < this.eps) {   // 到达目的地
            this.cells[0] = this.next_cell
            this.next_cell = null
            this.status = "idle"

            if (!this.check_tail_increasing()) {
                this.cells.pop()   //  蛇长不变，要去掉蛇尾
            }
        } else {
            const move_distance = this.speed * this.timedelta / 1000 // 每帧距离
            this.cells[0].x += move_distance * dx / distance
            this.cells[0].y += move_distance * dy / distance
            if (!this.check_tail_increasing()) {
                const len = this.cells.length
                const tail = this.cells[len - 1], tail_target = this.cells[len - 2]
                const tail_dx = tail_target.x - tail.x, tail_dy = tail_target.y - tail.y
                tail.x += move_distance * tail_dx / distance
                tail.y += move_distance * tail_dy / distance
            }
        }
    }
    update() {
        if (this.status === 'move') {
            this.update_move()
        }
        this.render()
    }
    render() {
        const L = this.gamemap.L
        const ctx = this.gamemap.ctx

        ctx.fillStyle = this.color
        if (this.status == "die") {
            ctx.fillStyle = "white"
        }

        for (const cell of this.cells) {
            ctx.beginPath()
            ctx.arc(cell.x * L, cell.y * L, L / 2 * 0.8, 0, Math.PI * 2)
            ctx.fill()
        }
        for (let i = 1; i < this.cells.length; i++) {
            const a = this.cells[i - 1], b = this.cells[i]
            if (Math.abs(a.x - b.x) < this.eps && Math.abs(a.y - b.y) < this.eps) {
                continue
            } else if (Math.abs(a.x - b.x) < this.eps) {
                ctx.fillRect((a.x - 0.4) * L, Math.min(a.y, b.y) * L, L * 0.8, Math.abs(a.y - b.y) * L)
            } else {
                ctx.fillRect(Math.min(a.x, b.x) * L, (a.y - 0.4) * L, Math.abs(a.x - b.x) * L, L * 0.8)
            }
        }
        ctx.fillStyle = 'black'
        for (let i = 0; i < 2; i++) {
            const eye_x = (this.cells[0].x + this.eye_dx[this.eye_direction][i] * 0.15) * L
            const eye_y = (this.cells[0].y + this.eye_dy[this.eye_direction][i] * 0.15) * L
            ctx.beginPath()
            ctx.arc(eye_x, eye_y, L * 0.065, 0, Math.PI * 2)
            ctx.fill()
        }
    }
}
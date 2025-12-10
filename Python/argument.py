def add_arguments(parser):
    '''
    Add your arguments here if needed. The TA will run test.py to load
    your default arguments.

    For example:
        parser.add_argument('--batch_size', type=int, default=32, help='batch size for training')
        parser.add_argument('--learning_rate', type=float, default=0.01, help='learning rate for training')
    '''
    parser.add_argument('--batch_size', type=int, default=32, help='batch size for training')
    parser.add_argument('--min_buff_size', type=int, default=5000, help='minimum buffer size to start training network')
    parser.add_argument('--max_buff_size', type=int, default=int(8e4), help='maximum buffer size')
    parser.add_argument('--learning_rate', type=float, default=2.5e-4, help='learning rate for training')
    parser.add_argument('--model_store_thresh', type=int, default=5000, help="Store the model as the target model after this many steps (gradient updates).")
    parser.add_argument('--episodes', type=int, default=50000, help="Update the model")
    parser.add_argument('--epsilon_warmup_steps', type=int, default=int(1e4), help="")
    parser.add_argument('--epsilon_decay_steps', type=int, default=int(3.5e6) , help="")
    parser.add_argument('--epsilon_start', type=float, default=1, help="")
    parser.add_argument('--epsilon_end', type=float, default=0.05, help="")
    parser.add_argument('--epsilon_lowest', type=float, default=0.001, help="")
    parser.add_argument('--epsilon_phase_len', type=int, default=1e3, help="")
    parser.add_argument('--gamma', type=float, default=0.99, help="")
    parser.add_argument('--checkpoint_short_steps', type=int, default=int(1e5), help="Short checkpoints are overwritten")
    parser.add_argument('--checkpoint_long_steps', type=int, default=int(2e6), help="Long checkpoints persist in storage.")
    parser.add_argument('--checkpoint_load_point', help="Either specify a checkpoint number or use \"latest\" keyword.")
    parser.add_argument('--model_name', default="alpha")
    parser.add_argument('--model_tag', default="best")

    return parser
